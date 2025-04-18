// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

#nullable enable

using System;
using SpacetimeDB.ClientApi;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.Types
{
    public sealed partial class RemoteReducers : RemoteBase
    {
        public delegate void IdentityDisconnectedHandler(ReducerEventContext ctx);
        public event IdentityDisconnectedHandler? OnIdentityDisconnected;

        public bool InvokeIdentityDisconnected(ReducerEventContext ctx, Reducer.IdentityDisconnected args)
        {
            if (OnIdentityDisconnected == null)
            {
                if (InternalOnUnhandledReducerError != null)
                {
                    switch (ctx.Event.Status)
                    {
                        case Status.Failed(var reason): InternalOnUnhandledReducerError(ctx, new Exception(reason)); break;
                        case Status.OutOfEnergy(var _): InternalOnUnhandledReducerError(ctx, new Exception("out of energy")); break;
                    }
                }
                return false;
            }
            OnIdentityDisconnected(
                ctx
            );
            return true;
        }
    }

    public abstract partial class Reducer
    {
        [SpacetimeDB.Type]
        [DataContract]
        public sealed partial class IdentityDisconnected : Reducer, IReducerArgs
        {
            string IReducerArgs.ReducerName => "identity_disconnected";
        }
    }
}
