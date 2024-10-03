// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;
using SpacetimeDB.ClientApi;
using System.Collections.Generic;

namespace SpacetimeDB.Types
{
	public sealed class RemoteTables
	{
	}

	public sealed class RemoteReducers : RemoteBase<DbConnection>
	{
		internal RemoteReducers(DbConnection conn) : base(conn) {}
	}

	public partial record EventContext : DbContext<RemoteTables>, IEventContext
	{
		public readonly RemoteReducers Reducers;
		public readonly Event<Reducer> Reducer;

		internal EventContext(DbConnection conn, Event<Reducer> reducer) : base(conn.RemoteTables)
		{
			Reducers = conn.RemoteReducers;
			Reducer = reducer;
		}
	}

	[Type]
	public partial record Reducer : TaggedEnum<(
		Unit StdbNone,
		Unit StdbIdentityConnected,
		Unit StdbIdentityDisconnected
	)>;
	public class DbConnection : DbConnectionBase<DbConnection, Reducer>
	{
		public readonly RemoteTables RemoteTables = new();
		public readonly RemoteReducers RemoteReducers;

		public DbConnection()
		{
			RemoteReducers = new(this);

		}

		protected override Reducer ToReducer(TransactionUpdate update)
		{
			var encodedArgs = update.ReducerCall.Args;
			return update.ReducerCall.ReducerName switch {
				"<none>" => new Reducer.StdbNone(default),
				"__identity_connected__" => new Reducer.StdbIdentityConnected(default),
				"__identity_disconnected__" => new Reducer.StdbIdentityDisconnected(default),
				"" => new Reducer.StdbNone(default),
				var reducer => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
			};
		}

		protected override IEventContext ToEventContext(Event<Reducer> reducerEvent) =>
		new EventContext(this, reducerEvent);

		protected override bool Dispatch(IEventContext context, Reducer reducer)
		{
			var eventContext = (EventContext)context;
			return reducer switch {
				Reducer.StdbNone or
				Reducer.StdbIdentityConnected or
				Reducer.StdbIdentityDisconnected => true,
				_ => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
			};
		}

		public SubscriptionBuilder<EventContext> SubscriptionBuilder() => new(this);
	}
}
