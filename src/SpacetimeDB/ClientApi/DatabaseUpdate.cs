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
	public partial class DatabaseUpdate
	{
		[DataMember(Name = "tables")]
		public System.Collections.Generic.List<SpacetimeDB.ClientApi.TableUpdate> Tables = new();
	}
}
