// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;

namespace SpacetimeDB.Types
{
	[SpacetimeDB.Type]
	public partial class SendMessageArgsStruct : IReducerArgs
	{
		string IReducerArgsBase.ReducerName => "send_message";
		bool IReducerArgs.InvokeHandler(EventContext ctx) => ctx.Reducers.InvokeSendMessage(ctx, this);

		public string Text = "";
	}
}
