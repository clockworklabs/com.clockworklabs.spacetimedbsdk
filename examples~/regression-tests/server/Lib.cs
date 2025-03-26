using SpacetimeDB;

public static partial class Module
{
    [SpacetimeDB.Table(Name = "ExampleData", Public = true)]
    public partial struct ExampleData
    {
        [SpacetimeDB.PrimaryKey]
        public uint Id;

        [SpacetimeDB.Index.BTree]
        public uint Indexed;
    }

    [SpacetimeDB.Reducer]
    public static void Delete(ReducerContext ctx, uint id)
    {
        ctx.Db.ExampleData.Id.Delete(id);
    }

    [SpacetimeDB.Reducer]
    public static void Add(ReducerContext ctx, uint id, uint indexed)
    {
        ctx.Db.ExampleData.Insert(new ExampleData { Id = id, Indexed = indexed });
    }
}
