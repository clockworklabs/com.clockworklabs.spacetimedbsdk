// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.

// <auto-generated />

#nullable enable

using System;
using SpacetimeDB;
using SpacetimeDB.BSATN;
using SpacetimeDB.ClientApi;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.Types
{
	public sealed partial class RemoteTables
	{
		public sealed class MessageHandle : RemoteTableHandle<EventContext, Message>
		{
			internal MessageHandle()
			{
			}
		}

		public readonly MessageHandle Message = new();
	}
}
