// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SpacetimeDB.ClientApi
{
	[DataContract]
	[SpacetimeDB.Type]
	public partial class ReducerCallInfo
	{
		[DataMember(Name = "reducer_name")]
		public string ReducerName = "";

		[DataMember(Name = "reducer_id")]
		public uint ReducerId;

		[DataMember(Name = "args")]
		public SpacetimeDB.ClientApi.EncodedValue Args = null!;

		[DataMember(Name = "request_id")]
		public uint RequestId;
	}
}
