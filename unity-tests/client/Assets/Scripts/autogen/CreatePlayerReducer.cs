// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;

namespace SpacetimeDB.Types
{
	[SpacetimeDB.Type]
	public partial class CreatePlayer : IReducerArgs
	{
		string IReducerArgs.ReducerName => "create_player";

		public string Name = "";
	}
}
