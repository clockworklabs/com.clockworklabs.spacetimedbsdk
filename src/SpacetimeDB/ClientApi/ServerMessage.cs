// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.
// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;

namespace SpacetimeDB.ClientApi
{
	[SpacetimeDB.Type]
	public partial record ServerMessage : SpacetimeDB.TaggedEnum<(
		SpacetimeDB.ClientApi.InitialSubscription InitialSubscription,
		SpacetimeDB.ClientApi.TransactionUpdate TransactionUpdate,
		SpacetimeDB.ClientApi.TransactionUpdateLight TransactionUpdateLight,
		SpacetimeDB.ClientApi.AfterConnecting AfterConnecting,
		SpacetimeDB.ClientApi.OneOffQueryResponse OneOffQueryResponse
	)>;
}
