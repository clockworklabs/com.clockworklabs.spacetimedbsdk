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
	public partial class BsatnRowList
	{
		[DataMember(Name = "size_hint")]
		public SpacetimeDB.Types.RowSizeHint SizeHint;
		[DataMember(Name = "rows_data")]
		public System.Collections.Generic.List<byte> RowsData;

		public BsatnRowList(
			SpacetimeDB.Types.RowSizeHint SizeHint,
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
