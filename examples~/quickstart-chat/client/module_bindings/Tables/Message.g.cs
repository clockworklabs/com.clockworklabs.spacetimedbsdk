// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

// This was generated using spacetimedb cli version 1.1.1 (commit bc3d453e871c797c17fdab2d772019832cd9b73e).

#nullable enable

using System;
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
            protected override string RemoteTableName => "message";

            internal MessageHandle(DbConnection conn) : base(conn)
            {
            }
        }

        public readonly MessageHandle Message;
    }
}
