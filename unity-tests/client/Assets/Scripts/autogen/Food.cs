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
	public partial class Food : IDatabaseRow
	{
		[DataMember(Name = "entity_id")]
		public uint EntityId;

		public Food(
			uint EntityId
		)
		{
			this.EntityId = EntityId;
		}

		public Food()
		{
		}

	}
}
