// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

#nullable enable

using System;
using SpacetimeDB.ClientApi;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.Types
{
    public sealed partial class RemoteReducers : RemoteBase<DbConnection>
    {
        public delegate void IdentityConnectedHandler(EventContext ctx);
        public event IdentityConnectedHandler? OnIdentityConnected;

        public bool InvokeIdentityConnected(EventContext ctx, Reducer.IdentityConnected args)
        {
            if (OnIdentityConnected == null) return false;
            OnIdentityConnected(
                ctx
            );
            return true;
        }
    }

    public abstract partial class Reducer
    {
        [SpacetimeDB.Type]
        [DataContract]
        public sealed partial class IdentityConnected : Reducer, IReducerArgs
        {
            string IReducerArgs.ReducerName => "identity_connected";
        }
    }
}
