// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

// This was generated using spacetimedb cli version 1.1.1 (commit bc3d453e871c797c17fdab2d772019832cd9b73e).

#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.ClientApi
{
    [SpacetimeDB.Type]
    [DataContract]
    public sealed partial class OneOffQuery
    {
        [DataMember(Name = "message_id")]
        public System.Collections.Generic.List<byte> MessageId;
        [DataMember(Name = "query_string")]
        public string QueryString;

        public OneOffQuery(
            System.Collections.Generic.List<byte> MessageId,
            string QueryString
        )
        {
            this.MessageId = MessageId;
            this.QueryString = QueryString;
        }

        public OneOffQuery()
        {
            this.MessageId = new();
            this.QueryString = "";
        }
    }
}
