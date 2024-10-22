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
	public partial class Player : IDatabaseRow
	{
		[DataMember(Name = "identity")]
		public SpacetimeDB.Identity Identity;
		[DataMember(Name = "player_id")]
		public uint PlayerId;
		[DataMember(Name = "name")]
		public string Name;

		public Player(
			SpacetimeDB.Identity Identity,
			uint PlayerId,
			string Name
		)
		{
			this.Identity = Identity;
			this.PlayerId = PlayerId;
			this.Name = Name;
		}

		public Player()
		{
			this.Identity = new();
			this.Name = "";
		}

	}
}
