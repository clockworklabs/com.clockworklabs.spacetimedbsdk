// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;
using SpacetimeDB.ClientApi;

namespace SpacetimeDB.Types
{
	public interface IReducerArgs : IReducerArgsBase
	{
		bool InvokeHandler(EventContext ctx);
	}

	public sealed class RemoteTables
	{
		public readonly RemoteTableHandle<EventContext, Message> Message = new();
		public readonly RemoteTableHandle<EventContext, User> User = new();
	}

	public sealed class RemoteReducers : RemoteBase<DbConnection>
	{
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

	public partial class EventContext : EventContextBase<RemoteTables, RemoteReducers>
	{
		public IReducerArgs? Args { get; }

		public string ReducerName => Args?.ReducerName ?? "<none>";

		public EventContext(DbConnection conn, TransactionUpdate update, IReducerArgs? args) : base(conn.RemoteTables, conn.RemoteReducers, update) => Args = args;

		public override bool InvokeHandler() => Args?.InvokeHandler(this) ?? false;
	}

	public class DbConnection : DbConnectionBase<DbConnection, EventContext>
	{
		public readonly RemoteTables RemoteTables = new();
		public readonly RemoteReducers RemoteReducers = new();

		public DbConnection()
		{
			RemoteReducers.Init(this);

			clientDB.AddTable<Message>();
			clientDB.AddTable<User>();
		}

		protected override EventContext ReducerEventFromDbEvent(TransactionUpdate update)
		{
			var encodedArgs = update.ReducerCall.Args;
			IReducerArgs? args = update.ReducerCall.ReducerName switch {
				"send_message" => BSATNHelpers.Decode<SendMessageArgsStruct>(encodedArgs),
				"set_name" => BSATNHelpers.Decode<SetNameArgsStruct>(encodedArgs),
				"<none>" => null,
				"__identity_connected__" => null,
				"__identity_disconnected__" => null,
				"" => null,
				var reducer => throw new ArgumentOutOfRangeException("Reducer", $"Unknown reducer {reducer}")
			};
			return new EventContext(this, update, args);
		}
	}
}
