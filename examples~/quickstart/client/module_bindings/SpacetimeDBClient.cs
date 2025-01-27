// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.

// <auto-generated />

#nullable enable

using System;
using SpacetimeDB.ClientApi;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.Types
{
    public sealed partial class RemoteReducers : RemoteBase<DbConnection>
    {
        internal RemoteReducers(DbConnection conn, SetReducerFlags SetReducerFlags) : base(conn) { this.SetCallReducerFlags = SetReducerFlags; }
        internal readonly SetReducerFlags SetCallReducerFlags;
    }

    public sealed partial class SetReducerFlags
    {
        internal SetReducerFlags() { }
    }

    public sealed record EventContext : DbContext<RemoteTables>, IEventContext
    {
        public readonly RemoteReducers Reducers;
        public readonly SetReducerFlags SetReducerFlags;
        public readonly Event<Reducer> Event;

        internal EventContext(DbConnection conn, Event<Reducer> reducerEvent) : base(conn.Db)
        {
            Reducers = conn.Reducers;
            SetReducerFlags = conn.SetReducerFlags;
            Event = reducerEvent;
        }
    }

    public abstract partial class Reducer
    {
        private Reducer() { }

        public sealed class StdbNone : Reducer { }
    }

    public sealed class DbConnection : DbConnectionBase<DbConnection, Reducer>
    {
        public readonly RemoteTables Db = new();
        public readonly RemoteReducers Reducers;
        public readonly SetReducerFlags SetReducerFlags;

        public DbConnection()
        {
            SetReducerFlags = new();
            Reducers = new(this, this.SetReducerFlags);

            clientDB.AddTable<Message>("message", Db.Message);
            clientDB.AddTable<User>("user", Db.User);
        }

        protected override Reducer ToReducer(TransactionUpdate update)
        {
            var encodedArgs = update.ReducerCall.Args;
            return update.ReducerCall.ReducerName switch
            {
                "identity_connected" => BSATNHelpers.Decode<Reducer.IdentityConnected>(encodedArgs),
                "identity_disconnected" => BSATNHelpers.Decode<Reducer.IdentityDisconnected>(encodedArgs),
                "send_message" => BSATNHelpers.Decode<Reducer.SendMessage>(encodedArgs),
                "set_name" => BSATNHelpers.Decode<Reducer.SetName>(encodedArgs),
                "<none>" or "" => new Reducer.StdbNone(),
                var reducer => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
            };
        }

        protected override IEventContext ToEventContext(Event<Reducer> reducerEvent) =>
        new EventContext(this, reducerEvent);

        protected override bool Dispatch(IEventContext context, Reducer reducer)
        {
            var eventContext = (EventContext)context;
            return reducer switch
            {
                Reducer.IdentityConnected args => Reducers.InvokeIdentityConnected(eventContext, args),
                Reducer.IdentityDisconnected args => Reducers.InvokeIdentityDisconnected(eventContext, args),
                Reducer.SendMessage args => Reducers.InvokeSendMessage(eventContext, args),
                Reducer.SetName args => Reducers.InvokeSetName(eventContext, args),
                Reducer.StdbNone => true,
                _ => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
            };
        }

        public SubscriptionBuilder<EventContext> SubscriptionBuilder() => new(this);
    }
}
