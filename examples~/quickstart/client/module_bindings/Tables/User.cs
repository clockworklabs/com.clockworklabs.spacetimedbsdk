// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN RUST INSTEAD.

// <auto-generated />

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
        public sealed class UserHandle : RemoteTableHandle<EventContext, User>
        {
            public override void InternalInvokeValueInserted(IStructuralReadWrite row)
            {
                var value = (User)row;
                Identity.Cache[value.Identity] = value;
            }

            public override void InternalInvokeValueDeleted(IStructuralReadWrite row)
            {
                Identity.Cache.Remove(((User)row).Identity);
            }

            public sealed class IdentityUniqueIndex
            {
                internal readonly Dictionary<SpacetimeDB.Identity, User> Cache = new(16);

                public User? Find(SpacetimeDB.Identity value)
                {
                    Cache.TryGetValue(value, out var r);
                    return r;
                }
            }

            public IdentityUniqueIndex Identity = new();

            internal UserHandle()
            {
            }

            public override object GetPrimaryKey(IStructuralReadWrite row) => ((User)row).Identity;
        }

        public readonly UserHandle User = new();
    }
}
