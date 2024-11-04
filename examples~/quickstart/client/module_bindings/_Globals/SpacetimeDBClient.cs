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
		public class MessageHandle : RemoteTableHandle<EventContext, Message>
		{
			internal MessageHandle()
			{
			}

		}

		public readonly MessageHandle Message = new();

		public class UserHandle : RemoteTableHandle<EventContext, User>
		{
			private static Dictionary<SpacetimeDB.Identity, User> Identity_Index = new(16);

			public override void InternalInvokeValueInserted(IDatabaseRow row)
			{
				var value = (User)row;
				Identity_Index[value.Identity] = value;
			}

			public override void InternalInvokeValueDeleted(IDatabaseRow row)
			{
				Identity_Index.Remove(((User)row).Identity);
			}

			public readonly ref struct IdentityUniqueIndex
			{
				public User? Find(SpacetimeDB.Identity value)
				{
					Identity_Index.TryGetValue(value, out var r);
					return r;
				}

			}

			public IdentityUniqueIndex Identity => new();

			internal UserHandle()
			{
			}
			public override object GetPrimaryKey(IDatabaseRow row) => ((User)row).Identity;

		}

		public readonly UserHandle User = new();

	}

	public sealed class RemoteReducers : RemoteBase<DbConnection>
	{
		internal RemoteReducers(DbConnection conn, SetReducerFlags SetReducerFlags) : base(conn) { this.SetCallReducerFlags = SetReducerFlags; }
		internal readonly SetReducerFlags SetCallReducerFlags;
		public delegate void SendMessageHandler(EventContext ctx, string text);
		public event SendMessageHandler? OnSendMessage;

		public void SendMessage(string text)
		{
			conn.InternalCallReducer(new SendMessage { Text = text }, this.SetCallReducerFlags.SendMessageFlags);
		}

		public bool InvokeSendMessage(EventContext ctx, SendMessage args)
		{
			if (OnSendMessage == null) return false;
			OnSendMessage(
				ctx,
				args.Text
			);
			return true;
		}
		public delegate void SetNameHandler(EventContext ctx, string name);
		public event SetNameHandler? OnSetName;

		public void SetName(string name)
		{
			conn.InternalCallReducer(new SetName { Name = name }, this.SetCallReducerFlags.SetNameFlags);
		}

		public bool InvokeSetName(EventContext ctx, SetName args)
		{
			if (OnSetName == null) return false;
			OnSetName(
				ctx,
				args.Name
			);
			return true;
		}
	}

	public sealed class SetReducerFlags
	{
		internal SetReducerFlags() { }
		internal CallReducerFlags SendMessageFlags;
		public void SendMessage(CallReducerFlags flags) { this.SendMessageFlags = flags; }
		internal CallReducerFlags SetNameFlags;
		public void SetName(CallReducerFlags flags) { this.SetNameFlags = flags; }
	}

	public partial record EventContext : DbContext<RemoteTables>, IEventContext
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

	[Type]
	public partial record Reducer : TaggedEnum<(
		SendMessage SendMessage,
		SetName SetName,
		Unit StdbNone,
		Unit StdbIdentityConnected,
		Unit StdbIdentityDisconnected
	)>;
	public class DbConnection : DbConnectionBase<DbConnection, Reducer>
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
				"send_message" => new Reducer.SendMessage(BSATNHelpers.Decode<SendMessage>(encodedArgs)),
				"set_name" => new Reducer.SetName(BSATNHelpers.Decode<SetName>(encodedArgs)),
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
			return reducer switch
			{
				Reducer.SendMessage(var args) => Reducers.InvokeSendMessage(eventContext, args),
				Reducer.SetName(var args) => Reducers.InvokeSetName(eventContext, args),
				Reducer.StdbNone or
				Reducer.StdbIdentityConnected or
				Reducer.StdbIdentityDisconnected => true,
				_ => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
			};
		}

		public SubscriptionBuilder<EventContext> SubscriptionBuilder() => new(this);
	}
}
