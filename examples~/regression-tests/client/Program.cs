/// Check that an issue with BTreeIndexes retaining rows after they have been deleted is resolved.

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

uint waiting = 0;
bool applied = false;
SubscriptionHandle? handle = null;

void OnConnected(DbConnection conn, Identity identity, string authToken)
{
    Log.Debug("Connected to btree-repro");
    handle = conn.SubscriptionBuilder()
        .OnApplied(OnSubscriptionApplied)
        .OnError((ctx, err) =>
        {
            throw err;
        })
        .Subscribe(["SELECT * FROM ExampleData"]);

    conn.Reducers.OnAdd += (ReducerEventContext ctx, uint id, uint indexed) =>
    {
        Log.Info("Got Add callback");
        waiting--;
        ValidateBTreeIndexes(ctx);
    };

    conn.Reducers.OnDelete += (ReducerEventContext ctx, uint id) =>
    {
        Log.Info("Got Delete callback");
        waiting--;
        ValidateBTreeIndexes(ctx);
    };
}

const uint MAX_ID = 10;

void ValidateBTreeIndexes(IRemoteDbContext conn)
{
    Log.Debug("Checking indexes...");
    foreach (var data in conn.Db.ExampleData.Iter())
    {
        Debug.Assert(conn.Db.ExampleData.Indexed.Filter(data.Id).Contains(data));
    }
    var outOfIndex = conn.Db.ExampleData.Iter().ToHashSet();

    for (uint i = 0; i < MAX_ID; i++)
    {
        foreach (var data in conn.Db.ExampleData.Indexed.Filter(i))
        {
            Debug.Assert(outOfIndex.Contains(data));
        }
    }
}

void OnSubscriptionApplied(SubscriptionEventContext context)
{
    Log.Debug("Calling Add");
    context.Reducers.Add(1, 1);
    applied = true;
    waiting++;
    Log.Debug("Calling Delete");
    context.Reducers.Delete(1);
    waiting++;
    Log.Debug("Calling Add");
    context.Reducers.Add(1, 1);
    applied = true;
    waiting++;
    Log.Debug("Calling Unsubscribe");
    handle?.UnsubscribeThen((ctx) =>
    {
        Log.Debug("Received Unsubscribe");
        ValidateBTreeIndexes(ctx);
        waiting--;
    });
}

System.AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    Log.Exception($"Unhandled exception: {sender} {args}");
    Environment.Exit(1);
};
var db = ConnectToDB();
var start = DateTime.Now;
while (!applied || waiting > 0)
{
    db.FrameTick();
    Thread.Sleep(100);
    if ((DateTime.Now - start).Seconds > 10)
    {
        Log.Error("Timeout, all events should have elapsed in 10 seconds!");
        Environment.Exit(1);
    }
}
Log.Info("Success");
Environment.Exit(0);