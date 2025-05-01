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
        internal static SpacetimeDBNetworkManager? _instance;

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

        public void Start()
        {
            StartCoroutine(EndOfFrameTickLoop());
        }

        private readonly List<IDbConnection> activeConnections = new();

        public bool AddConnection(IDbConnection conn)
        {
            if (activeConnections.Contains(conn))
            {
                return false;
            }
            activeConnections.Add(conn);
            return true;

        }

        public bool RemoveConnection(IDbConnection conn)
        {
            return activeConnections.Remove(conn);
        }
        
        private void ForEachConnection(Action<IDbConnection> action)
        {
            // It's common to call disconnect from Update, which will then modify the ActiveConnections collection,
            // therefore we must reverse-iterate the list of connections.
            for (var x = activeConnections.Count - 1; x >= 0; x--)
            {
                action(activeConnections[x]);
            }
        }

        private void Update() => ForEachConnection(conn => conn.FrameTick());

        /// The idea behind this is:
        ///
        /// If there was a response from the DB during the current frame the pre-processing of that response can
        /// start as early as possible.
        /// By the time the next frame is processed the result may already be available,
        /// saving up to one frame of latency.
        private System.Collections.IEnumerator EndOfFrameTickLoop()
        {
            var waitForEndOfFrame = new WaitForEndOfFrame();
            while (Application.isPlaying)
            {
                yield return waitForEndOfFrame;

                // this is called after the current frame is rendered but before the next frame is started
                // see: https://docs.unity3d.com/6000.0/Documentation/Manual/execution-order.html
                ForEachConnection(conn => conn.LateFrameTick());
            }
        }

        /// <inheritdoc cref="EndOfFrameTickLoop"/>
        /// This is called after all regular gameplay code has finished but before rendering has occurred.
        private void LateUpdate() => ForEachConnection(conn => conn.LateFrameTick());

        private void OnDestroy() => ForEachConnection(conn => conn.Disconnect());
    }
}
#endif
