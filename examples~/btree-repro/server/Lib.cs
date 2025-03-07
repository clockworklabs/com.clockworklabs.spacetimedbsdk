using SpacetimeDB;

public static partial class Module
{
    [SpacetimeDB.Table(Name = "ExampleData")]
    public partial struct ExampleData
    {
        [SpacetimeDB.AutoInc]
        [SpacetimeDB.PrimaryKey]
        public uint Id;

        [SpacetimeDB.Index.BTree]
        public uint Indexed;
    }

    [SpacetimeDB.Reducer]
    public static void Clear(ReducerContext ctx)
    {
        ctx.Db.ExampleData.Indexed.Delete((0, uint.MaxValue));
    }

    [SpacetimeDB.Reducer]
    public static void Add(ReducerContext ctx, uint id, uint indexed)
    {
        ctx.Db.ExampleData.Insert(new ExampleData { Id = id, Indexed = indexed });
    }
}
