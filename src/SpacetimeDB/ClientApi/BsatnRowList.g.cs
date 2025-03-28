// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.ClientApi
{
    [SpacetimeDB.Type]
    [DataContract]
    public sealed partial class BsatnRowList
    {
        [DataMember(Name = "size_hint")]
        public RowSizeHint SizeHint;
        [DataMember(Name = "rows_data")]
        public System.Collections.Generic.List<byte> RowsData;

        public BsatnRowList(
            RowSizeHint SizeHint,
            System.Collections.Generic.List<byte> RowsData
        )
        {
            this.SizeHint = SizeHint;
            this.RowsData = RowsData;
        }

        public BsatnRowList()
        {
            this.SizeHint = null!;
            this.RowsData = new();
        }
    }
}
