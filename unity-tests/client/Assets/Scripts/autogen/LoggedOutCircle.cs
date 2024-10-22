// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.Types
{
	[SpacetimeDB.Type]
	[DataContract]
	public partial class LoggedOutCircle : IDatabaseRow
	{
		[DataMember(Name = "logged_out_id")]
		public uint LoggedOutId;
		[DataMember(Name = "player_id")]
		public uint PlayerId;
		[DataMember(Name = "circle")]
		public SpacetimeDB.Types.Circle Circle;
		[DataMember(Name = "entity")]
		public SpacetimeDB.Types.Entity Entity;

		public LoggedOutCircle(
			uint LoggedOutId,
			uint PlayerId,
			SpacetimeDB.Types.Circle Circle,
			SpacetimeDB.Types.Entity Entity
		)
		{
			this.LoggedOutId = LoggedOutId;
			this.PlayerId = PlayerId;
			this.Circle = Circle;
			this.Entity = Entity;
		}

		public LoggedOutCircle()
		{
			this.Circle = new();
			this.Entity = new();
		}

	}
}
