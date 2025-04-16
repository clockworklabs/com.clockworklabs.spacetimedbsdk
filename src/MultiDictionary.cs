using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace SpacetimeDB
{
    /// <summary>
    /// A dictionary that may have multiple copies of a key-value pair.
    /// Note that a particular key only maps to one value -- it is a logical error
    /// to insert the same key with different values.
    /// 
    /// You MUST use the <c>MultiDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)</c>
    /// constructor to construct this -- it is a struct for performance reasons, but the default constructor creates an invalid state.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal struct MultiDictionary<TKey, TValue> : IEquatable<MultiDictionary<TKey, TValue>>
    {
        // The actual data.
        readonly Dictionary<TKey, (TValue Value, uint Multiplicity)> RawDict;
        readonly IEqualityComparer<TValue> ValueComparer;

        /// <summary>
        /// Construct a MultiDictionary.
        /// 
        /// This is the only valid constructor for a Multidictionary - using the parameterless constructor
        /// will result in null pointer errors. But we can't enforce this because of Unity.
        /// </summary>
        /// <param name="keyComparer"></param>
        public MultiDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            RawDict = new(keyComparer);
            ValueComparer = valueComparer;
        }

        public static MultiDictionary<TKey, TValue> FromEnumerable(IEnumerable<KeyValuePair<TKey, TValue>> enumerable, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            var result = new MultiDictionary<TKey, TValue>(keyComparer, valueComparer);
            foreach (var item in enumerable)
            {
                result.Add(item.Key, item.Value);
            }
            return result;
        }

        /// <summary>
        /// Return the count WITHOUT multiplicities.
        /// This is mathematically unnatural, but cheap.
        /// </summary>
        public readonly uint CountDistinct => (uint)RawDict.Count;

        /// <summary>
        /// Return the count WITH multiplicities.
        /// </summary>
        public readonly uint Count => RawDict.Select(item => item.Value.Multiplicity).Aggregate(0u, (a, b) => a + b);

        /// <summary>
        /// Add a key-value-pair to the multidictionary.
        /// If the key is already present, its associated value must satisfy
        /// keyComparer.Equals(value, item.Value).
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Whether the key is entirely new to the dictionary. If it was already present, we assert that the old value is equal to the new value.</returns>
        public bool Add(TKey key, TValue value)
        {
            if (value == null)
            {
                throw new NullReferenceException("Null values are forbidden in multidictionary");
            }
            Debug.Assert(RawDict != null);
            Debug.Assert(key != null);
            if (RawDict.TryGetValue(key, out var result))
            {
                Debug.Assert(ValueComparer.Equals(value, result.Value), "Added key-value pair with mismatched value to existing data");
                RawDict[key] = (value, result.Multiplicity + 1);
                return false;
            }
            else
            {
                RawDict[key] = (value, 1);
                return true;
            }
        }

        /// <summary>
        /// Completely clear the multidictionary.
        /// </summary>
        public void Clear()
        {
            RawDict.Clear();
        }

        /// <summary>
        /// Whether the multidictionary contains any copies of an item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (RawDict.TryGetValue(item.Key, out var result))
            {
                return ValueComparer.Equals(item.Value, result.Value);
            }
            return false;
        }

        /// <summary>
        /// Remove a key from the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Whether the last copy of the key was removed.</returns>
        public bool Remove(TKey key, out TValue row)
        {
            if (RawDict.TryGetValue(key, out var result))
            {
                row = result.Value;
                if (result.Multiplicity == 1)
                {
                    RawDict.Remove(key);
                    return true;
                }
                else
                {
                    RawDict[key] = (result.Value, result.Multiplicity - 1);
                    return false;
                }
            }
            row = default!; // uhh, this might be null. Good thing it's an internal method?
            return false;
        }

        public bool Equals(MultiDictionary<TKey, TValue> other)
        {
            foreach (var item in RawDict)
            {
                var (key, (value, multiplicity)) = item;
                if (other.RawDict.TryGetValue(key, out var otherVM))
                {
                    var (otherValue, otherMultiplicity) = otherVM;
                    if (!(ValueComparer.Equals(value, otherValue) && multiplicity == otherMultiplicity))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public readonly IEnumerable<TValue> Values
        {
            get
            {

                return RawDict.Select(item => item.Value.Value);
            }
        }

        public readonly IEnumerable<KeyValuePair<TKey, TValue>> Entries
        {
            get
            {
                return RawDict.Select(item => new KeyValuePair<TKey, TValue>(item.Key, item.Value.Value));
            }
        }

        /// <summary>
        /// Iterate the rows that will be removed when `delta` is applied.
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        public readonly IEnumerable<KeyValuePair<TKey, TValue>> WillRemove(MultiDictionaryDelta<TKey, TValue> delta)
        {
            var self = this;
            return delta.Entries.Where(their =>
            {
                if (their.Value.IsValueChange)
                {
                    // Updates don't count as removals.
                    return false;
                }
                var theirNonUpdate = their.Value.NonValueChange;
                if (theirNonUpdate.Delta >= 0)
                {
                    // Adds can't result in removals.
                    return false;
                }
                if (self.RawDict.TryGetValue(their.Key, out var mine))
                {
                    var resultMultiplicity = (int)mine.Multiplicity + theirNonUpdate.Delta;
                    return resultMultiplicity <= 0; // if < 0, we have a problem, but that's caught in Apply.
                }
                else
                {
                    Log.Warn($"Want to remove row with key {their.Key}, but it doesn't exist!");
                    return false;
                }
            }).Select(entry => new KeyValuePair<TKey, TValue>(entry.Key, entry.Value.NonValueChange.Value));
        }

        /// <summary>
        /// Apply a collection of changes to a multidictionary.
        /// </summary>
        /// <param name="delta">The changes to apply.</param>
        /// <param name="wasInserted">Will be populated with inserted KVPs.</param>
        /// <param name="wasUpdated">Will be populated with updated KVPs.</param>
        /// <param name="wasRemoved">Will be populated with removed KVPs.</param>
        public void Apply(MultiDictionaryDelta<TKey, TValue> delta, List<KeyValuePair<TKey, TValue>> wasInserted, List<(TKey Key, TValue OldValue, TValue NewValue)> wasUpdated, List<KeyValuePair<TKey, TValue>> wasRemoved)
        {
            foreach (var (key, their) in delta.Entries)
            {
                if (RawDict.TryGetValue(key, out var my))
                {
                    if (their.IsValueChange)
                    {
                        var (before, after) = their.ValueChange;
                        Debug.Assert(ValueComparer.Equals(my.Value, before.Value));
                        var reducedMultiplicity = (int)my.Multiplicity + before.Delta;
                        if (reducedMultiplicity != 0)
                        {
                            PseudoThrow($"Attempted to apply {their} to {my}, but this resulted in a multiplicity of {reducedMultiplicity}, failing to correctly remove the row before applying the update");
                        }
                        RawDict[key] = (after.Value, (uint)after.Delta);

                        wasUpdated.Add((key, my.Value, after.Value));
                    }
                    else
                    { // !their.IsValueChange
                        var theirDelta = their.NonValueChange;
                        Debug.Assert(ValueComparer.Equals(my.Value, theirDelta.Value), $"mismatched value change: {my.Value} {theirDelta.Value} {their}");
                        var newMultiplicity = (int)my.Multiplicity + theirDelta.Delta;
                        if (newMultiplicity > 0)
                        {
                            // Update the count, NOT dispatching an update event.

                            // It sort of matters if we use my.Value or their.Value here:
                            // They may satisfy `Equals` but not actually have equal pointers.
                            // We'd prefer to keep pointers stable if they don't need to change.
                            // So even though my.Value and theirValue are "equal", prefer using my.Value.
                            RawDict[key] = (my.Value, (uint)newMultiplicity);
                        }
                        else // if (newMultiplicity <= 0)
                        {
                            // This is a removal.
                            if (newMultiplicity < 0)
                            {
                                PseudoThrow($"Internal error: Removing row with key {key} {-theirDelta.Delta} times, but it is only present {my.Multiplicity} times.");
                            }
                            RawDict.Remove(key);
                            wasRemoved.Add(new(key, theirDelta.Value));
                        }
                    }
                }
                else
                {
                    // Key is not present in map.
                    if (their.IsValueChange)
                    {
                        PseudoThrow($"Internal error: Can't perform a value change on a nonexistent key {key} (change: {their}).");
                    }
                    else
                    {
                        var theirDelta = their.NonValueChange;

                        if (theirDelta.Delta == 0)
                        {
                            // Hmm.
                            // This is not actually a problem.
                            // Do nothing.
                        }
                        else if (theirDelta.Delta < 0)
                        {
                            PseudoThrow($"Internal error: Can't remove nonexistent key {theirDelta.Value}");
                        }
                        else
                        {
                            RawDict[key] = (theirDelta.Value, (uint)theirDelta.Delta);
                            wasInserted.Add(new(key, theirDelta.Value));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Raise a debug assertion failure in debug mode, otherwise just warn and keep going.
        /// </summary>
        /// <param name="message"></param>
        private void PseudoThrow(string message)
        {
            Log.Warn(message);
            Debug.Assert(false, message);
        }

        public override string ToString()
        {
            StringBuilder result = new();
            result.Append("SpacetimeDB.MultiDictionary { ");
            foreach (var item in RawDict)
            {
                result.Append($"({item.Key}: {item.Value.Value}) x {item.Value.Multiplicity}, ");
            }
            result.Append("}");
            return result.ToString();
        }

    }

    /// <summary>
    /// A bulk change to a multidictionary. Allows both adding and removing rows.
    /// 
    /// You MUST use the <c>MultiDictionaryDelta(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)</c>
    /// to construct this -- it is a struct for performance reasons, but the default constructor creates an invalid collection!
    /// 
    /// Can be applied to a multidictionary, and also inspected before application to see
    /// what rows will be deleted. (This is used for OnBeforeDelete.)
    /// 
    /// The order of operations applied to a MultiDictionaryDelta does not matter.
    /// No matter the order of Add and Remove calls on a delta, when the Delta is applied,
    /// the result will be the same, as long as the Add and Remove *counts* for each KeyValuePair are
    /// the same.
    /// (This means that this is a "conflict-free replicated data type", unlike MultiDictionary.)
    /// (MultiDictionary would also be "conflict-free" if it didn't support Remove.)
    ///
    /// The delta may include value updates.
    /// When applied, the delta must maintain the invariant of MultiDictionary that each key maps to exactly one value.
    /// For example, if the target dictionary has the state:
    /// <c>(k1: v1) (k1: v1)</c>
    /// Then a delta must remove both of these key-value pairs if it wishes to assign a new value to <c>k1</c>.
    ///
    /// Each key can be associated with at most two values in a MultiDictionaryDelta.
    /// For example, <c>-(k1: v1) +(k1: v2) -(k1: v2) +(k1: v3)</c> is NOT a valid MultiDictionaryDelta.
    /// 
    /// When removing a row for an update, it is legal for the passed value to be equal to EITHER the old value or the new value. 
    /// (This is because I'm not sure what SpacetimeDB core does.)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal struct MultiDictionaryDelta<TKey, TValue> : IEquatable<MultiDictionaryDelta<TKey, TValue>>
    {
        /// <summary>
        /// A change to an individual value associated to a key.
        /// </summary>
        public struct ValueDelta
        {
            /// <summary>
            /// The value stored.
            /// </summary>
            public TValue Value;

            /// <summary>
            /// The change in multiplicity of the value.
            /// </summary>
            public int Delta;

            public ValueDelta(TValue Value, int Delta)
            {
                this.Value = Value;
                this.Delta = Delta;
            }

            public override string ToString()
            {
                return $"{Value} x ({Delta})";
            }

            public bool Equals(ValueDelta other, IEqualityComparer<TValue> equalityComparer) =>
                equalityComparer.Equals(Value, other.Value) && Delta == other.Delta;
        }

        /// <summary>
        /// A change to a key-value pair.
        /// 
        /// - If the value associated to the key changes, then <c>.IsValueChange == true</c>; use <c>.ValueChange</c> to
        ///     get the values before and after the change, along with their multiplicities.
        ///     
        /// - If the value associated to the key does not change, the key-value pair can still have multiplicity changes.
        ///     Use <c>.NonValueChange</c> to get at this multiplicity information.
        /// 
        /// </summary>
        public struct KeyDelta
        {
            // In general, we don't know which of these are the Before and After until
            // after we've accumulated all of the Adds and Removes for the relevant key.
            // So, this information is exposed to clients through the getters below.
            ValueDelta D1;
            ValueDelta? D2;

            public KeyDelta(ValueDelta delta)
            {
                D1 = delta;
                D2 = null;
            }

            /// <summary>
            /// If this KeyDelta is a value change -- that is, it removes one value some number of times and adds another some number of times.
            /// (If it isn't a value change, it just will consist of adds or removes for a single value).
            /// </summary>
            public bool IsValueChange
            {
                get => D2 != null && D1.Delta < 0 && D2.Value.Delta > 0;
            }

            /// <summary>
            /// The deltas in the case of this KeyDelta being a value change.
            /// Guarantees <c>Before.Delta < 0 && After.Delta > 0</c>.
            /// </summary>
            public (ValueDelta Before, ValueDelta After) ValueChange
            {
                get
                {
                    Debug.Assert(IsValueChange);
                    return (D1, D2!.Value);
                }
            }

            /// <summary>
            /// If !IsUpdate, this gives you the single relevant ValueDelta for this key.
            /// </summary>
            public ValueDelta NonValueChange
            {
                get
                {
                    Debug.Assert(!IsValueChange);
                    if (D2 == null)
                    {
                        return D1;
                    }
                    else
                    {
                        // Now we're in a weird place.
                        // We're not an update, but D2 is initialized.
                        // This means that at least one of D1 or D2 has Delta == 0.
                        // If exactly one of them has Delta == 0, we're okay:
                        if (D1.Delta == 0 && D2.Value.Delta != 0)
                        {
                            return D2.Value;
                        }
                        else if (D1.Delta != 0 && D2.Value.Delta == 0)
                        {
                            return D1;
                        }
                        // In this case, something strange is going on: both values have the same sign.
                        // There's nothing sensible to do here, and this represents a server-side error, so just throw.
                        throw new InvalidOperationException($"Called NonValueChange on a ValueDelta in an ambiguous state: {this}");
                    }
                }
            }

            public void Add(TValue value, IEqualityComparer<TValue> equalityComparer)
            {
                if (equalityComparer.Equals(value, D1.Value))
                {
                    D1.Delta += 1;
                }
                else if (D2 == null)
                {
                    D2 = new(value, +1);
                }
                else
                {
                    var d2 = D2.Value;
                    Debug.Assert(equalityComparer.Equals(value, d2.Value));
                    d2.Delta += 1;
                    D2 = d2;
                }
                Normalize();
            }

            public void Remove(TValue value, IEqualityComparer<TValue> equalityComparer)
            {
                if (equalityComparer.Equals(value, D1.Value))
                {
                    D1.Delta -= 1;
                }
                else if (D2 == null)
                {
                    D2 = new(value, -1);
                }
                else
                {
                    var newD2 = D2.Value;
                    Debug.Assert(equalityComparer.Equals(value, newD2.Value));
                    newD2.Delta -= 1;
                    D2 = newD2;
                }
                Normalize();
            }

            private void Normalize()
            {
                if (D2 != null && D2.Value.Delta < D1.Delta)
                {
                    var tmp = D2.Value;
                    D2 = D1;
                    D1 = tmp;
                }
            }

            public bool Equals(KeyDelta other, IEqualityComparer<TValue> equalityComparer)
            {
                if (IsValueChange != other.IsValueChange) return false;
                if (IsValueChange)
                {
                    var asUpdate = ValueChange;
                    var otherAsUpdate = other.ValueChange;
                    return asUpdate.Before.Equals(otherAsUpdate.Before, equalityComparer) &&
                        asUpdate.After.Equals(otherAsUpdate.After, equalityComparer);
                }
                else
                {
                    return NonValueChange.Equals(other.NonValueChange, equalityComparer);
                }
            }

            public override string ToString()
            {
                if (D2 == null)
                {
                    return D1.ToString();
                }
                else
                {
                    return $"({D1}, {D2})";
                }
            }
        }

        /// <summary>
        /// For each key, track its old and new values.
        /// Also track the number of times it has been removed and inserted.
        /// We keep these separate so that we can debug-assert that a KVP has been removed enough times (in case
        /// there are multiple copies of the KVP in the map we get applied to.)
        /// </summary>
        readonly Dictionary<TKey, KeyDelta> RawDict;

        readonly IEqualityComparer<TValue> ValueComparer;

        /// <summary>
        /// Construct a MultiDictionaryDelta.
        /// 
        /// This is the ONLY valid constructor for a MultiDictionaryDelta - using the parameterless constructor
        /// will result in null pointer errors. But we can't enforce this because of Unity.
        /// </summary>
        /// <param name="keyComparer"></param>

        public MultiDictionaryDelta(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            RawDict = new(keyComparer);
            ValueComparer = valueComparer;
        }

        /// <summary>
        /// Add a key-value-pair to the multidictionary.
        /// If the key is already present, its associated value must satisfy
        /// keyComparer.Equals(value, item.Value).
        /// </summary>
        /// <param name="item"></param>
        public void Add(TKey key, TValue value)
        {
            if (value == null)
            {
                throw new NullReferenceException("Null values are forbidden in multidictionary");
            }
            Debug.Assert(RawDict != null);
            Debug.Assert(key != null);
            KeyDelta result;
            if (RawDict.TryGetValue(key, out result))
            {
                result.Add(value, ValueComparer);
            }
            else
            {
                result = new(new(value, +1));
            }
            RawDict[key] = result;
        }

        /// <summary>
        /// Completely clear the multidictionary.
        /// </summary>
        public void Clear()
        {
            RawDict.Clear();
        }

        /// <summary>
        /// Remove a key from the dictionary.
        /// </summary>
        /// <param name="key"></param>
        public void Remove(TKey key, TValue value)
        {
            KeyDelta result;
            if (RawDict.TryGetValue(key, out result))
            {
                result.Remove(value, ValueComparer);
            }
            else
            {
                result = new(new(value, -1));
            }
            RawDict[key] = result;
        }

        public override string ToString()
        {
            StringBuilder result = new();
            result.Append("SpacetimeDB.MultiDictionaryDelta { ");
            foreach (var item in RawDict)
            {
                result.Append($"({item.Key}: {item.Value}, ");
            }
            result.Append("}");
            return result.ToString();
        }

        public bool Equals(MultiDictionaryDelta<TKey, TValue> other)
        {
            foreach (var item in RawDict)
            {
                var (key, my) = item;
                if (other.RawDict.TryGetValue(key, out var their))
                {
                    if (!their.Equals(my, ValueComparer))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public readonly IEnumerable<KeyValuePair<TKey, KeyDelta>> Entries
        {
            get
            {
                return RawDict;
            }
        }
    }
}