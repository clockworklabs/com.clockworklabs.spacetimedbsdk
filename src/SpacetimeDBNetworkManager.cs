#if UNITY_5_3_OR_NEWER
using System;
using System.Collections.Generic;
using SpacetimeDB;
using UnityEngine;

namespace SpacetimeDB
{
    // This class is only used in Unity projects.
    // Attach this to a GameObject in your scene to use SpacetimeDB.
    public class SpacetimeDBNetworkManager : MonoBehaviour
    {
        private static SpacetimeDBNetworkManager? _instance;

        public void Awake()
        {
            // Ensure that users don't create several SpacetimeDBNetworkManager instances.
            // We're using a global (static) list of active connections and we don't want several instances to walk over it several times.
            if (_instance != null)
            {
                throw new InvalidOperationException("SpacetimeDBNetworkManager is a singleton and should only be attached once.");
            }
            else
            {
                _instance = this;
            }
        }

        internal static HashSet<IDbConnection> ActiveConnections = new();

        private List<IDbConnection> cache = new List<IDbConnection>();

        private void ForEachConnection(Action<IDbConnection> action)
        {
            // TODO(jdetter): We're doing this for now because we can't break the API during a minor release but
            // in the future we should just change ActiveConnections to be a list so that we can reverse-iterate
            // through it.
            cache.Clear();
            cache.AddRange(ActiveConnections);

            // It's common to call disconnect from Update, which will then modify the ActiveConnections collection,
            // therefore we must reverse-iterate the list of connections.
            for (var x = cache.Count - 1; x >= 0; x--)
            {
                action(cache[x]);
            }
        }

        private void Update() => ForEachConnection(conn => conn.FrameTick());
        private void OnDestroy() => ForEachConnection(conn => conn.Disconnect());
    }
}
#endif
