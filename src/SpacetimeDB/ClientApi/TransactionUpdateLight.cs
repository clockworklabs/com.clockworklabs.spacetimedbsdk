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
	public partial class TransactionUpdateLight
	{
		[DataMember(Name = "request_id")]
		public uint RequestId;
		[DataMember(Name = "update")]
		public SpacetimeDB.ClientApi.DatabaseUpdate Update;

		public TransactionUpdateLight(
			uint RequestId,
			SpacetimeDB.ClientApi.DatabaseUpdate Update
		)
		{
			this.RequestId = RequestId;
			this.Update = Update;
		}

		public TransactionUpdateLight()
		{
			this.Update = new();
		}

	}
}
