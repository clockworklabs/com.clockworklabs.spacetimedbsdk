// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.ClientApi
{
	[SpacetimeDB.Type]
	[DataContract]
	public partial class OneOffQuery
	{
		[DataMember(Name = "message_id")]
		public byte[] MessageId;
		[DataMember(Name = "query_string")]
		public string QueryString;

		public OneOffQuery(
			byte[] MessageId,
			string QueryString
		)
		{
			this.MessageId = MessageId;
			this.QueryString = QueryString;
		}

		public OneOffQuery()
		{
			this.MessageId = Array.Empty<byte>();
			this.QueryString = "";
		}

	}
}
