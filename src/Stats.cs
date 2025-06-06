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
        public TimeSpan Window = new TimeSpan(0, 0, 5 /* seconds */);

        public DateTime LastReset = DateTime.UtcNow;

        /// <summary>
        /// The fastest request OF ALL TIME.
        /// We keep data for less time than we used to -- having this around catches outliers that may be problematic.
        /// </summary>
        public (TimeSpan Duration, string Metadata)? AllTimeMin
        {
            get; private set;
        }

        /// <summary>
        /// The slowest request OF ALL TIME.
        /// We keep data for less time than we used to -- having this around catches outliers that may be problematic.
        /// </summary>
        public (TimeSpan Duration, string Metadata)? AllTimeMax
        {
            get; private set;
        }

        // The min and max for the previous window.
        private int LastWindowSamples = 0;
        private (TimeSpan Duration, string Metadata)? LastWindowMin;
        private (TimeSpan Duration, string Metadata)? LastWindowMax;

        // The min and max for the current window.
        private int ThisWindowSamples = 0;
        private (TimeSpan Duration, string Metadata)? ThisWindowMin;
        private (TimeSpan Duration, string Metadata)? ThisWindowMax;

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

        internal void InsertRequest(TimeSpan duration, string metadata)
        {
            var sample = (duration, metadata);

            if (AllTimeMin == null || AllTimeMin.Value.Duration > duration)
            {
                AllTimeMin = sample;
            }
            if (AllTimeMax == null || AllTimeMax.Value.Duration < duration)
            {
                AllTimeMax = sample;
            }
            if (ThisWindowMin == null || AllTimeMin.Value.Duration > duration)
            {
                ThisWindowMin = sample;
            }
            if (ThisWindowMax == null || AllTimeMax.Value.Duration < duration)
            {
                ThisWindowMax = sample;
            }
            ThisWindowSamples += 1;

            if (LastReset < DateTime.UtcNow - Window)
            {
                LastReset = DateTime.UtcNow;
                LastWindowMax = ThisWindowMax;
                LastWindowMin = ThisWindowMin;
                LastWindowSamples = ThisWindowSamples;
                ThisWindowMax = null;
                ThisWindowMin = null;
                ThisWindowSamples = 0;
            }
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
            if (LastWindowMin != null && LastWindowMax != null)
            {
                return (LastWindowMin.Value, LastWindowMax.Value);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the number of samples in the window.
        /// </summary>
        /// <returns></returns>
        public int GetSampleCount() => LastWindowSamples;

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
