using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SpacetimeDB
{
    internal readonly struct ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public static readonly ByteArrayComparer Instance = new();

        public bool Equals(byte[]? left, byte[]? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null || left.Length != right.Length)
            {
                return false;
            }

            return EqualsUnvectorized(left, right);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool EqualsUnvectorized(byte[] left, byte[] right)
        {
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(byte[] obj)
        {
            int hash = 17;
            foreach (byte b in obj)
            {
                hash = hash * 31 + b;
            }
            return hash;
        }
    }
}
