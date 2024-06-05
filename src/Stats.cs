using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SpacetimeDB
{
    public class NetworkRequestTracker
    {
        private readonly ConcurrentQueue<(DateTime, TimeSpan, string)> _requestDurations = new();

        private uint _nextRequestId;
        private readonly Dictionary<uint, (DateTime, string)> _requests = new();

        public uint StartTrackingRequest(string metadata = "")
        {
            // Record the start time of the request
            var newRequestId = ++_nextRequestId;
            _requests[newRequestId] = (DateTime.UtcNow, metadata);
            return newRequestId;
        }

        public bool FinishTrackingRequest(uint requestId)
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
            InsertRequest(entry.Item1, entry.Item2);
            return true;
        }

        public void InsertRequest(TimeSpan duration, string metadata)
        {
            _requestDurations.Enqueue((DateTime.UtcNow, duration, metadata));
        }

        public void InsertRequest(DateTime start, string metadata)
        {
            InsertRequest(DateTime.UtcNow - start, metadata);
        }

        public ((TimeSpan, string), (TimeSpan, string)) GetMinMaxTimes(int lastSeconds)
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-lastSeconds);

            if (!_requestDurations.Where(x => x.Item1 >= cutoff).Select(x => (x.Item2, x.Item3)).Any())
            {
                return ((TimeSpan.Zero, ""), (TimeSpan.Zero, ""));
            }

            var min = _requestDurations.Where(x => x.Item1 >= cutoff).Select(x => (x.Item2, x.Item3)).Min();
            var max = _requestDurations.Where(x => x.Item1 >= cutoff).Select(x => (x.Item2, x.Item3)).Max();

            return (min, max);
        }

        public int GetSampleCount() => _requestDurations.Count;
        public int GetRequestsAwaitingResponse() => _requests.Count;
    }


    public class Stats
    {
        public NetworkRequestTracker ReducerRequestTracker = new();
        public NetworkRequestTracker OneOffRequestTracker = new();
        public NetworkRequestTracker SubscriptionRequestTracker = new();
        public NetworkRequestTracker AllReducersTracker = new();
        public NetworkRequestTracker ParseMessageTracker = new();
    }
}
