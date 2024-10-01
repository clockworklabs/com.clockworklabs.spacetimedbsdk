// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using System.Collections.Generic;

using SpacetimeDB.ClientApi;

namespace SpacetimeDB.Types
{
	public sealed class RemoteTables
	{
		public class MessageHandle : RemoteTableHandle<EventContext, Message> {
            public IEnumerable<Message> FilterBySender(SpacetimeDB.Identity value) {
                return Query(x => x.Sender == value);
            }

            public IEnumerable<Message> FilterBySent(ulong value) {
                return Query(x => x.Sent == value);
            }

            public IEnumerable<Message> FilterByText(string value) {
                return Query(x => x.Text == value);
            }
        }

        public class UserHandle : RemoteTableHandle<EventContext, User> {
			public override object? GetPrimaryKey(IDatabaseRow row) => ((User)row).Identity;

            private Dictionary<SpacetimeDB.Identity, User> Identity_Index = new(16);

            public override void InternalInvokeValueInserted(IDatabaseRow row) {
                var value = (User)row;
                Identity_Index[value.Identity] = value;
            }

            public override void InternalInvokeValueDeleted(IDatabaseRow row) {
                Identity_Index.Remove(((User)row).Identity);
            }

            public User? FindByIdentity(SpacetimeDB.Identity value) {
                Identity_Index.TryGetValue(value, out var r);
                return r;
            }

            public IEnumerable<User> FilterByIdentity(SpacetimeDB.Identity value) {
                if (FindByIdentity(value) is { } found) {
                    yield return found;
                }
            }

            public IEnumerable<User> FilterByOnline(bool value) {
                return Query(x => x.Online == value);
            }
        }

        public readonly MessageHandle Message = new();
		public readonly UserHandle User = new();
	}

	public sealed class RemoteReducers : RemoteBase<DbConnection>
	{
		internal RemoteReducers(DbConnection conn) : base(conn) {}

		public delegate void SendMessageHandler(EventContext ctx, string text);
		public event SendMessageHandler? OnSendMessage;

		public void SendMessage(string text)
		{
			conn.InternalCallReducer(new SendMessageArgsStruct { Text = text });
		}

		public bool InvokeSendMessage(EventContext ctx, SendMessageArgsStruct args)
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
			conn.InternalCallReducer(new SetNameArgsStruct { Name = name });
		}

		public bool InvokeSetName(EventContext ctx, SetNameArgsStruct args)
		{
			if (OnSetName == null) return false;
			OnSetName(
				ctx,
				args.Name
			);
			return true;
		}
	}

	public partial record EventContext : DbContext<RemoteTables>, IEventContext {
		public readonly RemoteReducers Reducers;
		public readonly Event<Reducer> Reducer;

		internal EventContext(DbConnection conn, Event<Reducer> reducer) : base(conn.RemoteTables) {
			Reducers = conn.RemoteReducers;
			Reducer = reducer;
		}
	}

	[Type]
	public partial record Reducer : TaggedEnum<(
		SendMessageArgsStruct SendMessage,
		SetNameArgsStruct SetName,
		Unit IdentityConnected,
		Unit IdentityDisconnected
    )>;

	public class DbConnection : DbConnectionBase<DbConnection, Reducer>
	{
		public readonly RemoteTables RemoteTables = new();
		public readonly RemoteReducers RemoteReducers;

		public DbConnection()
		{
			RemoteReducers = new(this);

			clientDB.AddTable<Message>("Message", RemoteTables.Message);
			clientDB.AddTable<User>("User", RemoteTables.User);
		}

		protected override Reducer ToReducer(TransactionUpdate update)
		{
			var encodedArgs = update.ReducerCall.Args;
			return update.ReducerCall.ReducerName switch {
				"send_message" => new Reducer.SendMessage(BSATNHelpers.Decode<SendMessageArgsStruct>(encodedArgs)),
				"set_name" => new Reducer.SetName(BSATNHelpers.Decode<SetNameArgsStruct>(encodedArgs)),
				"__identity_connected__" => new Reducer.IdentityConnected(default),
				"__identity_disconnected__" => new Reducer.IdentityDisconnected(default),
				var reducer => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
			};
		}

		protected override IEventContext ToEventContext(Event<Reducer> reducerEvent) {
			return new EventContext(this, reducerEvent);
		}

		protected override bool Dispatch(IEventContext context, Reducer reducer) {
			var eventContext = (EventContext)context;
			return reducer switch {
				Reducer.SendMessage(var args) => RemoteReducers.InvokeSendMessage(eventContext, args),
				Reducer.SetName(var args) => RemoteReducers.InvokeSetName(eventContext, args),
				Reducer.IdentityConnected => true,
				Reducer.IdentityDisconnected => true,
				_ => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
			};
		}
	}
}
