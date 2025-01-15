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
	public partial class SubscribeApplied
	{
		[DataMember(Name = "request_id")]
		public uint RequestId;
		[DataMember(Name = "total_host_execution_duration_micros")]
		public ulong TotalHostExecutionDurationMicros;
		[DataMember(Name = "query_id")]
		public SpacetimeDB.ClientApi.QueryId QueryId;
		[DataMember(Name = "rows")]
		public SpacetimeDB.ClientApi.SubscribeRows Rows;

		public SubscribeApplied(
			uint RequestId,
			ulong TotalHostExecutionDurationMicros,
			SpacetimeDB.ClientApi.QueryId QueryId,
			SpacetimeDB.ClientApi.SubscribeRows Rows
		)
		{
			this.RequestId = RequestId;
			this.TotalHostExecutionDurationMicros = TotalHostExecutionDurationMicros;
			this.QueryId = QueryId;
			this.Rows = Rows;
		}

		public SubscribeApplied()
		{
			this.QueryId = new();
			this.Rows = new();
		}

	}
}
