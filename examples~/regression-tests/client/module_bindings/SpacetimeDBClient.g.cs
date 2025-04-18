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
        internal RemoteReducers(DbConnection conn, SetReducerFlags flags) : base(conn) => SetCallReducerFlags = flags;
        internal readonly SetReducerFlags SetCallReducerFlags;
        internal event Action<ReducerEventContext, Exception>? InternalOnUnhandledReducerError;
    }

    public sealed partial class RemoteTables : RemoteTablesBase
    {
        public RemoteTables(DbConnection conn)
        {
            AddTable(ExampleData = new(conn));
        }
    }

    public sealed partial class SetReducerFlags { }

    public interface IRemoteDbContext : IDbContext<RemoteTables, RemoteReducers, SetReducerFlags, SubscriptionBuilder>
    {
        public event Action<ReducerEventContext, Exception>? OnUnhandledReducerError;
    }

    public sealed class EventContext : IEventContext, IRemoteDbContext
    {
        private readonly DbConnection conn;

        /// <summary>
        /// The event that caused this callback to run.
        /// </summary>
        public readonly Event<Reducer> Event;

        /// <summary>
        /// Access to tables in the client cache, which stores a read-only replica of the remote database state.
        ///
        /// The returned <c>DbView</c> will have a method to access each table defined by the module.
        /// </summary>
        public RemoteTables Db => conn.Db;
        /// <summary>
        /// Access to reducers defined by the module.
        ///
        /// The returned <c>RemoteReducers</c> will have a method to invoke each reducer defined by the module,
        /// plus methods for adding and removing callbacks on each of those reducers.
        /// </summary>
        public RemoteReducers Reducers => conn.Reducers;
        /// <summary>
        /// Access to setters for per-reducer flags.
        ///
        /// The returned <c>SetReducerFlags</c> will have a method to invoke,
        /// for each reducer defined by the module,
        /// which call-flags for the reducer can be set.
        /// </summary>
        public SetReducerFlags SetReducerFlags => conn.SetReducerFlags;
        /// <summary>
        /// Returns <c>true</c> if the connection is active, i.e. has not yet disconnected.
        /// </summary>
        public bool IsActive => conn.IsActive;
        /// <summary>
        /// Close the connection.
        ///
        /// Throws an error if the connection is already closed.
        /// </summary>
        public void Disconnect()
        {
            conn.Disconnect();
        }
        /// <summary>
        /// Start building a subscription.
        /// </summary>
        /// <returns>A builder-pattern constructor for subscribing to queries,
        /// causing matching rows to be replicated into the client cache.</returns>
        public SubscriptionBuilder SubscriptionBuilder() => conn.SubscriptionBuilder();
        /// <summary>
        /// Get the <c>Identity</c> of this connection.
        ///
        /// This method returns null if the connection was constructed anonymously
        /// and we have not yet received our newly-generated <c>Identity</c> from the host.
        /// </summary>
        public Identity? Identity => conn.Identity;
        /// <summary>
        /// Get this connection's <c>ConnectionId</c>.
        /// </summary>
        public ConnectionId ConnectionId => conn.ConnectionId;
        /// <summary>
        /// Register a callback to be called when a reducer with no handler returns an error.
        /// </summary>
        public event Action<ReducerEventContext, Exception>? OnUnhandledReducerError
        {
            add => Reducers.InternalOnUnhandledReducerError += value;
            remove => Reducers.InternalOnUnhandledReducerError -= value;
        }

        internal EventContext(DbConnection conn, Event<Reducer> Event)
        {
            this.conn = conn;
            this.Event = Event;
        }
    }

    public sealed class ReducerEventContext : IReducerEventContext, IRemoteDbContext
    {
        private readonly DbConnection conn;
        /// <summary>
        /// The reducer event that caused this callback to run.
        /// </summary>
        public readonly ReducerEvent<Reducer> Event;

        /// <summary>
        /// Access to tables in the client cache, which stores a read-only replica of the remote database state.
        ///
        /// The returned <c>DbView</c> will have a method to access each table defined by the module.
        /// </summary>
        public RemoteTables Db => conn.Db;
        /// <summary>
        /// Access to reducers defined by the module.
        ///
        /// The returned <c>RemoteReducers</c> will have a method to invoke each reducer defined by the module,
        /// plus methods for adding and removing callbacks on each of those reducers.
        /// </summary>
        public RemoteReducers Reducers => conn.Reducers;
        /// <summary>
        /// Access to setters for per-reducer flags.
        ///
        /// The returned <c>SetReducerFlags</c> will have a method to invoke,
        /// for each reducer defined by the module,
        /// which call-flags for the reducer can be set.
        /// </summary>
        public SetReducerFlags SetReducerFlags => conn.SetReducerFlags;
        /// <summary>
        /// Returns <c>true</c> if the connection is active, i.e. has not yet disconnected.
        /// </summary>
        public bool IsActive => conn.IsActive;
        /// <summary>
        /// Close the connection.
        ///
        /// Throws an error if the connection is already closed.
        /// </summary>
        public void Disconnect()
        {
            conn.Disconnect();
        }
        /// <summary>
        /// Start building a subscription.
        /// </summary>
        /// <returns>A builder-pattern constructor for subscribing to queries,
        /// causing matching rows to be replicated into the client cache.</returns>
        public SubscriptionBuilder SubscriptionBuilder() => conn.SubscriptionBuilder();
        /// <summary>
        /// Get the <c>Identity</c> of this connection.
        ///
        /// This method returns null if the connection was constructed anonymously
        /// and we have not yet received our newly-generated <c>Identity</c> from the host.
        /// </summary>
        public Identity? Identity => conn.Identity;
        /// <summary>
        /// Get this connection's <c>ConnectionId</c>.
        /// </summary>
        public ConnectionId ConnectionId => conn.ConnectionId;
        /// <summary>
        /// Register a callback to be called when a reducer with no handler returns an error.
        /// </summary>
        public event Action<ReducerEventContext, Exception>? OnUnhandledReducerError
        {
            add => Reducers.InternalOnUnhandledReducerError += value;
            remove => Reducers.InternalOnUnhandledReducerError -= value;
        }

        internal ReducerEventContext(DbConnection conn, ReducerEvent<Reducer> reducerEvent)
        {
            this.conn = conn;
            Event = reducerEvent;
        }
    }

    public sealed class ErrorContext : IErrorContext, IRemoteDbContext
    {
        private readonly DbConnection conn;
        /// <summary>
        /// The <c>Exception</c> that caused this error callback to be run.
        /// </summary>
        public readonly Exception Event;
        Exception IErrorContext.Event
        {
            get
            {
                return Event;
            }
        }

        /// <summary>
        /// Access to tables in the client cache, which stores a read-only replica of the remote database state.
        ///
        /// The returned <c>DbView</c> will have a method to access each table defined by the module.
        /// </summary>
        public RemoteTables Db => conn.Db;
        /// <summary>
        /// Access to reducers defined by the module.
        ///
        /// The returned <c>RemoteReducers</c> will have a method to invoke each reducer defined by the module,
        /// plus methods for adding and removing callbacks on each of those reducers.
        /// </summary>
        public RemoteReducers Reducers => conn.Reducers;
        /// <summary>
        /// Access to setters for per-reducer flags.
        ///
        /// The returned <c>SetReducerFlags</c> will have a method to invoke,
        /// for each reducer defined by the module,
        /// which call-flags for the reducer can be set.
        /// </summary>
        public SetReducerFlags SetReducerFlags => conn.SetReducerFlags;
        /// <summary>
        /// Returns <c>true</c> if the connection is active, i.e. has not yet disconnected.
        /// </summary>
        public bool IsActive => conn.IsActive;
        /// <summary>
        /// Close the connection.
        ///
        /// Throws an error if the connection is already closed.
        /// </summary>
        public void Disconnect()
        {
            conn.Disconnect();
        }
        /// <summary>
        /// Start building a subscription.
        /// </summary>
        /// <returns>A builder-pattern constructor for subscribing to queries,
        /// causing matching rows to be replicated into the client cache.</returns>
        public SubscriptionBuilder SubscriptionBuilder() => conn.SubscriptionBuilder();
        /// <summary>
        /// Get the <c>Identity</c> of this connection.
        ///
        /// This method returns null if the connection was constructed anonymously
        /// and we have not yet received our newly-generated <c>Identity</c> from the host.
        /// </summary>
        public Identity? Identity => conn.Identity;
        /// <summary>
        /// Get this connection's <c>ConnectionId</c>.
        /// </summary>
        public ConnectionId ConnectionId => conn.ConnectionId;
        /// <summary>
        /// Register a callback to be called when a reducer with no handler returns an error.
        /// </summary>
        public event Action<ReducerEventContext, Exception>? OnUnhandledReducerError
        {
            add => Reducers.InternalOnUnhandledReducerError += value;
            remove => Reducers.InternalOnUnhandledReducerError -= value;
        }

        internal ErrorContext(DbConnection conn, Exception error)
        {
            this.conn = conn;
            Event = error;
        }
    }

    public sealed class SubscriptionEventContext : ISubscriptionEventContext, IRemoteDbContext
    {
        private readonly DbConnection conn;

        /// <summary>
        /// Access to tables in the client cache, which stores a read-only replica of the remote database state.
        ///
        /// The returned <c>DbView</c> will have a method to access each table defined by the module.
        /// </summary>
        public RemoteTables Db => conn.Db;
        /// <summary>
        /// Access to reducers defined by the module.
        ///
        /// The returned <c>RemoteReducers</c> will have a method to invoke each reducer defined by the module,
        /// plus methods for adding and removing callbacks on each of those reducers.
        /// </summary>
        public RemoteReducers Reducers => conn.Reducers;
        /// <summary>
        /// Access to setters for per-reducer flags.
        ///
        /// The returned <c>SetReducerFlags</c> will have a method to invoke,
        /// for each reducer defined by the module,
        /// which call-flags for the reducer can be set.
        /// </summary>
        public SetReducerFlags SetReducerFlags => conn.SetReducerFlags;
        /// <summary>
        /// Returns <c>true</c> if the connection is active, i.e. has not yet disconnected.
        /// </summary>
        public bool IsActive => conn.IsActive;
        /// <summary>
        /// Close the connection.
        ///
        /// Throws an error if the connection is already closed.
        /// </summary>
        public void Disconnect()
        {
            conn.Disconnect();
        }
        /// <summary>
        /// Start building a subscription.
        /// </summary>
        /// <returns>A builder-pattern constructor for subscribing to queries,
        /// causing matching rows to be replicated into the client cache.</returns>
        public SubscriptionBuilder SubscriptionBuilder() => conn.SubscriptionBuilder();
        /// <summary>
        /// Get the <c>Identity</c> of this connection.
        ///
        /// This method returns null if the connection was constructed anonymously
        /// and we have not yet received our newly-generated <c>Identity</c> from the host.
        /// </summary>
        public Identity? Identity => conn.Identity;
        /// <summary>
        /// Get this connection's <c>ConnectionId</c>.
        /// </summary>
        public ConnectionId ConnectionId => conn.ConnectionId;
        /// <summary>
        /// Register a callback to be called when a reducer with no handler returns an error.
        /// </summary>
        public event Action<ReducerEventContext, Exception>? OnUnhandledReducerError
        {
            add => Reducers.InternalOnUnhandledReducerError += value;
            remove => Reducers.InternalOnUnhandledReducerError -= value;
        }

        internal SubscriptionEventContext(DbConnection conn)
        {
            this.conn = conn;
        }
    }

    /// <summary>
    /// Builder-pattern constructor for subscription queries.
    /// </summary>
    public sealed class SubscriptionBuilder
    {
        private readonly IDbConnection conn;

        private event Action<SubscriptionEventContext>? Applied;
        private event Action<ErrorContext, Exception>? Error;

        /// <summary>
        /// Private API, use <c>conn.SubscriptionBuilder()</c> instead.
        /// </summary>
        public SubscriptionBuilder(IDbConnection conn)
        {
            this.conn = conn;
        }

        /// <summary>
        /// Register a callback to run when the subscription is applied.
        /// </summary>
        public SubscriptionBuilder OnApplied(
            Action<SubscriptionEventContext> callback
        )
        {
            Applied += callback;
            return this;
        }

        /// <summary>
        /// Register a callback to run when the subscription fails.
        ///
        /// Note that this callback may run either when attempting to apply the subscription,
        /// in which case <c>Self::on_applied</c> will never run,
        /// or later during the subscription's lifetime if the module's interface changes,
        /// in which case <c>Self::on_applied</c> may have already run.
        /// </summary>
        public SubscriptionBuilder OnError(
            Action<ErrorContext, Exception> callback
        )
        {
            Error += callback;
            return this;
        }

        /// <summary>
        /// Subscribe to the following SQL queries.
        /// 
        /// This method returns immediately, with the data not yet added to the DbConnection.
        /// The provided callbacks will be invoked once the data is returned from the remote server.
        /// Data from all the provided queries will be returned at the same time.
        /// 
        /// See the SpacetimeDB SQL docs for more information on SQL syntax:
        /// <a href="https://spacetimedb.com/docs/sql">https://spacetimedb.com/docs/sql</a>
        /// </summary>
        public SubscriptionHandle Subscribe(
            string[] querySqls
        ) => new(conn, Applied, Error, querySqls);

        /// <summary>
        /// Subscribe to all rows from all tables.
        ///
        /// This method is intended as a convenience
        /// for applications where client-side memory use and network bandwidth are not concerns.
        /// Applications where these resources are a constraint
        /// should register more precise queries via <c>Self.Subscribe</c>
        /// in order to replicate only the subset of data which the client needs to function.
        ///
        /// This method should not be combined with <c>Self.Subscribe</c> on the same <c>DbConnection</c>.
        /// A connection may either <c>Self.Subscribe</c> to particular queries,
        /// or <c>Self.SubscribeToAllTables</c>, but not both.
        /// Attempting to call <c>Self.Subscribe</c>
        /// on a <c>DbConnection</c> that has previously used <c>Self.SubscribeToAllTables</c>,
        /// or vice versa, may misbehave in any number of ways,
        /// including dropping subscriptions, corrupting the client cache, or panicking.
        /// </summary>
        public void SubscribeToAllTables()
        {
            // Make sure we use the legacy handle constructor here, even though there's only 1 query.
            // We drop the error handler, since it can't be called for legacy subscriptions.
            new SubscriptionHandle(
                conn,
                Applied,
                new string[] { "SELECT * FROM *" }
            );
        }
    }

    public sealed class SubscriptionHandle : SubscriptionHandleBase<SubscriptionEventContext, ErrorContext>
    {
        /// <summary>
        /// Internal API. Construct <c>SubscriptionHandle</c>s using <c>conn.SubscriptionBuilder</c>.
        /// </summary>
        public SubscriptionHandle(IDbConnection conn, Action<SubscriptionEventContext>? onApplied, string[] querySqls) : base(conn, onApplied, querySqls)
        { }

        /// <summary>
        /// Internal API. Construct <c>SubscriptionHandle</c>s using <c>conn.SubscriptionBuilder</c>.
        /// </summary>
        public SubscriptionHandle(
            IDbConnection conn,
            Action<SubscriptionEventContext>? onApplied,
            Action<ErrorContext, Exception>? onError,
            string[] querySqls
        ) : base(conn, onApplied, onError, querySqls)
        { }
    }

    public abstract partial class Reducer
    {
        private Reducer() { }
    }

    public sealed class DbConnection : DbConnectionBase<DbConnection, RemoteTables, Reducer>
    {
        public override RemoteTables Db { get; }
        public readonly RemoteReducers Reducers;
        public readonly SetReducerFlags SetReducerFlags = new();

        public DbConnection()
        {
            Db = new(this);
            Reducers = new(this, SetReducerFlags);
        }

        protected override Reducer ToReducer(TransactionUpdate update)
        {
            var encodedArgs = update.ReducerCall.Args;
            return update.ReducerCall.ReducerName switch
            {
                "Add" => BSATNHelpers.Decode<Reducer.Add>(encodedArgs),
                "Delete" => BSATNHelpers.Decode<Reducer.Delete>(encodedArgs),
                "ThrowError" => BSATNHelpers.Decode<Reducer.ThrowError>(encodedArgs),
                var reducer => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
            };
        }

        protected override IEventContext ToEventContext(Event<Reducer> Event) =>
        new EventContext(this, Event);

        protected override IReducerEventContext ToReducerEventContext(ReducerEvent<Reducer> reducerEvent) =>
        new ReducerEventContext(this, reducerEvent);

        protected override ISubscriptionEventContext MakeSubscriptionEventContext() =>
        new SubscriptionEventContext(this);

        protected override IErrorContext ToErrorContext(Exception exception) =>
        new ErrorContext(this, exception);

        protected override bool Dispatch(IReducerEventContext context, Reducer reducer)
        {
            var eventContext = (ReducerEventContext)context;
            return reducer switch
            {
                Reducer.Add args => Reducers.InvokeAdd(eventContext, args),
                Reducer.Delete args => Reducers.InvokeDelete(eventContext, args),
                Reducer.ThrowError args => Reducers.InvokeThrowError(eventContext, args),
                _ => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
            };
        }

        public SubscriptionBuilder SubscriptionBuilder() => new(this);
        public event Action<ReducerEventContext, Exception> OnUnhandledReducerError
        {
            add => Reducers.InternalOnUnhandledReducerError += value;
            remove => Reducers.InternalOnUnhandledReducerError -= value;
        }
    }
}
