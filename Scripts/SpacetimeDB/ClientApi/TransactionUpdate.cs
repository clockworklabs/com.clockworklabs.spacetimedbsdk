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
	[SpacetimeDB.Type]
	[DataContract]
	public partial class TransactionUpdate
	{
		[DataMember(Name = "status")]
		public SpacetimeDB.ClientApi.UpdateStatus Status = null!;

		[DataMember(Name = "timestamp")]
		public SpacetimeDB.ClientApi.Timestamp Timestamp = new();

		[DataMember(Name = "caller_identity")]
		public SpacetimeDB.Identity CallerIdentity = new();

		[DataMember(Name = "caller_address")]
		public SpacetimeDB.Address CallerAddress = new();

		[DataMember(Name = "reducer_call")]
		public SpacetimeDB.ClientApi.ReducerCallInfo ReducerCall = new();

		[DataMember(Name = "energy_quanta_used")]
		public SpacetimeDB.ClientApi.EnergyQuanta EnergyQuantaUsed = new();

		[DataMember(Name = "host_execution_duration_micros")]
		public ulong HostExecutionDurationMicros;
	}
}
