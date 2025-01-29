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
        public delegate void SendMessageHandler(EventContext ctx, string text);
        public event SendMessageHandler? OnSendMessage;

        public void SendMessage(string text)
        {
            conn.InternalCallReducer(new Reducer.SendMessage(text), this.SetCallReducerFlags.SendMessageFlags);
        }

        public bool InvokeSendMessage(EventContext ctx, Reducer.SendMessage args)
        {
            if (OnSendMessage == null) return false;
            OnSendMessage(
                ctx,
                args.Text
            );
            return true;
        }
    }

    public abstract partial class Reducer
    {
        [SpacetimeDB.Type]
        [DataContract]
        public sealed partial class SendMessage : Reducer, IReducerArgs
        {
            [DataMember(Name = "text")]
            public string Text;

            public SendMessage(string Text)
            {
                this.Text = Text;
            }

            public SendMessage()
            {
                this.Text = "";
            }

            string IReducerArgs.ReducerName => "send_message";
        }
    }

    public sealed partial class SetReducerFlags
    {
        internal CallReducerFlags SendMessageFlags;
        public void SendMessage(CallReducerFlags flags) { this.SendMessageFlags = flags; }
    }
}
