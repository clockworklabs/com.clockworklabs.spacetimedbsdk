using System;
using System.Collections.Generic;
using System.Linq;

namespace SpacetimeDB
{
    /// <summary>
    /// Class to track information about network requests and other internal statistics.
    /// </summary>
    public class NetworkRequestTracker
    {
        /// <summary>
        /// Keep this many seconds of network request data.
        /// You can update this value to change the policy for this tracker.
        /// </summary>
        public TimeSpan WINDOW = new TimeSpan(0, 0, 5 /* seconds */);

        /// <summary>
        /// Durations of completed requests in the time window.
        /// </summary>
        private readonly Queue<(TimeSpan Duration, DateTime End, string Metadata)> _requestDurations = new();

        /// <summary>
        /// Durations of completed requests in the time window, ordered by duration.
        /// </summary>
        private readonly SortedSet<(TimeSpan Duration, DateTime End, string Metadata)> _requestDurationsSorted = new(DurationsSortedComparer.INSTANCE);

        /// <summary>
        /// The fastest request OF ALL TIME.
        /// We keep data for less time than we used to -- having this around catches outliers that may be problematic.
        /// </summary>
        public (TimeSpan Duration, DateTime End, string Metadata)? AllTimeMin
        {
            get; private set;
        }

        /// <summary>
        /// The slowest request OF ALL TIME.
        /// We keep data for less time than we used to -- having this around catches outliers that may be problematic.
        /// </summary>
        public (TimeSpan Duration, DateTime End, string Metadata)? AllTimeMax
        {
            get; private set;
        }


        private class DurationsSortedComparer : IComparer<(TimeSpan Duration, DateTime End, string Metadata)>
        {
            public static DurationsSortedComparer INSTANCE = new();
            public int Compare((TimeSpan Duration, DateTime End, string Metadata) x, (TimeSpan Duration, DateTime End, string Metadata) y)
            {
                var result = x.Duration.CompareTo(y.Duration);
                if (result != 0) return result;
                return x.End.CompareTo(y.End);
            }
        }

        /// <summary>
        /// ID for the next in-flight request.
        /// </summary>
        private uint _nextRequestId;

        /// <summary>
        /// In-flight requests that have not yet finished running.
        /// </summary>
        private readonly Dictionary<uint, (DateTime Start, string Metadata)> _requests = new();

        internal uint StartTrackingRequest(string metadata = "")
        {
            // This method is called when the user submits a new request.
            // It's possible the user was naughty and did this off the main thread.
            // So, be a little paranoid and lock ourselves. Uncontended this will be pretty fast.
            lock (this)
            {
                // Get a new request ID.
                // Note: C# wraps by default, rather than throwing exception on overflow.
                // So, this class should work forever.
                var newRequestId = ++_nextRequestId;
                // Record the start time of the request.
                _requests[newRequestId] = (DateTime.UtcNow, metadata);
                return newRequestId;
            }
        }

        // The remaining methods in this class do not need to lock, since they are only called from OnProcessMessageComplete.

        internal bool FinishTrackingRequest(uint requestId)
        {
            if (!_requests.Remove(requestId, out var entry))
            {
                // TODO: When we implement requestId json support for SpacetimeDB this shouldn't happen anymore!
                // var minKey = _requests.Keys.Min();
                // entry = _requests[minKey];
                //
                // if (!_requests.Remove(minKey))
                // {
                //     return false;
                // }
                return false;
            }

            // Calculate the duration and add it to the queue
            InsertRequest(entry.Start, entry.Metadata);
            return true;
        }

        private void CleanUpOldMessages()
        {
            var threshold = DateTime.UtcNow - WINDOW;
            while (_requestDurations.TryPeek(out var front) && front.End < threshold)
            {
                _requestDurations.Dequeue();
                // Note: this remove may remove the wrong request if `front` was overwritten by another request.
                // We don't worry about this, assuming that durations are fine-grained enough to rarely collide.
                _requestDurationsSorted.Remove(front);
            }
        }

        internal void InsertRequest(TimeSpan duration, string metadata)
        {
            var now = DateTime.UtcNow;

            var sample = (duration, now, metadata);

            // Two elements of the sorted dictionary are considered equal if they have the same duration and timespan,
            // according to our IComparer.
            // In this case, which seems unlikely, just throw the duplicate out to avoid an error. 
            if (!_requestDurationsSorted.Contains(sample))
            {
                _requestDurations.Enqueue(sample);
                _requestDurationsSorted.Add(sample);
            }

            if (AllTimeMin == null || AllTimeMin.Value.Duration > duration)
            {
                AllTimeMin = sample;
            }
            if (AllTimeMax == null || AllTimeMax.Value.Duration < duration)
            {
                AllTimeMax = sample;
            }

            CleanUpOldMessages();
        }

        internal void InsertRequest(DateTime start, string metadata)
        {
            InsertRequest(DateTime.UtcNow - start, metadata);
        }

        /// <summary>
        /// Get the the minimum- and maximum-duration events in NetworkRequestTracker.WINDOW.
        /// </summary>
        /// <param name="_deprecated">Present for backwards-compatibility, does nothing.</param>
        public ((TimeSpan Duration, string Metadata) Min, (TimeSpan Duration, string Metadata) Max)? GetMinMaxTimes(int _deprecated = 0)
        {
            if (!_requestDurationsSorted.Any())
            {
                return null;
            }
            // Note: this is not LINQ Min, it's SortedSet Min, which is O(1).
            var min = _requestDurationsSorted.Min;
            // Similarly here.
            var max = _requestDurationsSorted.Max;

            return ((min.Duration, min.Metadata), (max.Duration, max.Metadata));
        }

        /// <summary>
        /// Get the number of samples in the window.
        /// </summary>
        /// <returns></returns>
        public int GetSampleCount() => _requestDurations.Count;

        /// <summary>
        /// Get the number of outstanding tracked requests.
        /// </summary>
        /// <returns></returns>
        public int GetRequestsAwaitingResponse() => _requests.Count;
    }

    public class Stats
    {
        public readonly NetworkRequestTracker ReducerRequestTracker = new();
        public readonly NetworkRequestTracker OneOffRequestTracker = new();
        public readonly NetworkRequestTracker SubscriptionRequestTracker = new();
        public readonly NetworkRequestTracker AllReducersTracker = new();
        public readonly NetworkRequestTracker ParseMessageTracker = new();
    }
}
