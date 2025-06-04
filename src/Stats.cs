using System;
using System.Collections.Generic;
using System.Linq;

namespace SpacetimeDB
{
    /// <summary>
    /// Class to track information about network requests and other internal statistics.
    /// Should only be accessed from the main thread.
    /// </summary>
    public class NetworkRequestTracker
    {
        /// <summary>
        /// Keep this many seconds of network request data.
        /// </summary>
        public static TimeSpan WINDOW = new TimeSpan(0, 0, 3 /* seconds */);

        /// <summary>
        /// Durations of completed requests in the time window.
        /// </summary>
        private readonly Queue<(DateTime End, TimeSpan Duration, string Metadata)> _requestDurations = new();

        /// <summary>
        /// Durations of completed requests in the time window, ordered by duration.
        /// </summary>
        private readonly SortedDictionary<TimeSpan, (DateTime End, string Metadata)> _requestDurationsSorted = new();

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
                _requestDurationsSorted.Remove(front.Duration);
            }
        }

        internal void InsertRequest(TimeSpan duration, string metadata)
        {
            _requestDurations.Enqueue((DateTime.UtcNow, duration, metadata));
        }

        internal void InsertRequest(DateTime start, string metadata)
        {
            InsertRequest(DateTime.UtcNow - start, metadata);
        }

        public ((TimeSpan Duration, string Metadata) Min, (TimeSpan Duration, string Metadata) Max)? GetMinMaxTimes(int lastSeconds)
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-lastSeconds);
            if (_requestDurationsSorted.Any())
            {
                return null;
            }
            var min = _requestDurationsSorted.Min();
            var max = _requestDurationsSorted.Max();

            return ((min.Key, min.Value.Metadata), (max.Key, max.Value.Metadata));
        }

        public int GetSampleCount() => _requestDurations.Count;
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
