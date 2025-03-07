// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Runtime.CompilerServices;
using SpacetimeDB;
using SpacetimeDB.Types;

const string HOST = "http://localhost:3000";
const string DBNAME = "btree-repro";

DbConnection ConnectToDB()
{
    DbConnection? conn = null;
    conn = DbConnection.Builder()
        .WithUri(HOST)
        .WithModuleName(DBNAME)
        .OnConnect(OnConnected)
        .OnConnectError((err) =>
        {
            throw err;
        })
        .OnDisconnect((conn, err) =>
        {
            if (err != null)
            {
                throw err;
            }
            else
            {
                throw new Exception("Unexpected disconnect");
            }
        })
        .Build();
    return conn;
}

// We assume we're the only one interacting with the server for this test.

void OnConnected(DbConnection conn, Identity identity, string authToken)
{
    Log.Debug("Connected to btree-repro");
    conn.Reducers.Clear();
    conn.SubscriptionBuilder()
        .OnApplied(OnSubscriptionApplied)
        .OnError((ctx, err) =>
        {
            throw err;
        })
        .Subscribe(["SELECT * FROM ExampleData"]);

    conn.Reducers.OnAdd += (ReducerEventContext ctx, uint id, uint indexed) =>
    {
        ValidateBTreeIndexes(ctx);
    };
}

void ValidateBTreeIndexes(IRemoteDbContext conn)
{
    Log.Debug("Checking indexes...");
    foreach (var data in conn.Db.ExampleData.Iter())
    {
        Debug.Assert(conn.Db.ExampleData.Indexed.Filter(data.Id).Contains(data));
    }
}

bool Done = false;

void OnSubscriptionApplied(SubscriptionEventContext context)
{
    context.Reducers.Add(0, 1);
    context.Reducers.Add(0, 1);
    context.Reducers.Add(0, 2);
    context.Reducers.Add(0, 1);
    context.Reducers.Add(0, 3);
    context.Reducers.Add(0, 1);
    Log.Debug("Success");
    Done = true;
}

var db = ConnectToDB();
while (!Done)
{
    db.FrameTick();
    Thread.Sleep(100);
}