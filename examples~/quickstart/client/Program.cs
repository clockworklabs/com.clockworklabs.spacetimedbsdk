﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using SpacetimeDB;
using SpacetimeDB.Types;

const string HOST = "http://localhost:3000";
const string DBNAME = "chatqs";

// our local client SpacetimeDB identity
Identity? local_identity = null;
// declare a thread safe queue to store commands
var input_queue = new ConcurrentQueue<(string Command, string Args)>();

void Main()
{
    AuthToken.Init(".spacetime_csharp_quickstart");

    // TODO: just do `var conn = DbConnection...` when OnConnect signature is fixed.
    DbConnection? conn = null;

    conn = DbConnection.Builder()
        .WithUri(HOST)
        .WithModuleName(DBNAME)
        //.WithToken(AuthToken.Token)
        .OnConnect(OnConnect)
        .OnConnectError(OnConnectError)
        .OnDisconnect(OnDisconnect)
        .Build();

    conn.Db.User.OnInsert += User_OnInsert;
    conn.Db.User.OnUpdate += User_OnUpdate;

    conn.Db.Message.OnInsert += Message_OnInsert;

    conn.Reducers.OnSetName += Reducer_OnSetNameEvent;
    conn.Reducers.OnSendMessage += Reducer_OnSendMessageEvent;

    // declare a threadsafe cancel token to cancel the process loop
    var cancellationTokenSource = new CancellationTokenSource();

    // spawn a thread to call process updates and process commands
    var thread = new Thread(() => ProcessThread(conn, cancellationTokenSource.Token));
    thread.Start();

    InputLoop();

    // this signals the ProcessThread to stop
    cancellationTokenSource.Cancel();
    thread.Join();
}

string UserNameOrIdentity(User user) => user.Name ?? user.Identity.ToString()[..8];

void User_OnInsert(EventContext ctx, User insertedValue)
{
    if (insertedValue.Online)
    {
        Console.WriteLine($"{UserNameOrIdentity(insertedValue)} is online");
    }
}

void User_OnUpdate(EventContext ctx, User oldValue, User newValue)
{
    if (oldValue.Name != newValue.Name)
    {
        Console.WriteLine($"{UserNameOrIdentity(oldValue)} renamed to {newValue.Name}");
    }
    if (oldValue.Online != newValue.Online)
    {
        if (newValue.Online)
        {
            Console.WriteLine($"{UserNameOrIdentity(newValue)} connected.");
        }
        else
        {
            Console.WriteLine($"{UserNameOrIdentity(newValue)} disconnected.");
        }
    }
}

void PrintMessage(RemoteTables tables, Message message)
{
    var sender = tables.User.Identity.Find(message.Sender);
    var senderName = "unknown";
    if (sender != null)
    {
        senderName = UserNameOrIdentity(sender);
    }

    Console.WriteLine($"{senderName}: {message.Text}");
}

void Message_OnInsert(EventContext ctx, Message insertedValue)
{

    if (ctx.Event is not Event<Reducer>.SubscribeApplied)
    {
        PrintMessage(ctx.Db, insertedValue);
    }
}

void Reducer_OnSetNameEvent(ReducerEventContext ctx, string name)
{
    var e = ctx.Event;
    if (e.CallerIdentity == local_identity && e.Status is Status.Failed(var error))
    {
        Console.Write($"Failed to change name to {name}: {error}");
    }
}

void Reducer_OnSendMessageEvent(ReducerEventContext ctx, string text)
{
    var e = ctx.Event;
    if (e.CallerIdentity == local_identity && e.Status is Status.Failed(var error))
    {
        Console.Write($"Failed to send message {text}: {error}");
    }
}

void OnConnect(DbConnection conn, Identity identity, string authToken)
{
    local_identity = identity;
    AuthToken.SaveToken(authToken);

    var subscription = conn.SubscriptionBuilder()
        .OnApplied(OnSubscriptionApplied)
        .Subscribe(new string[] {
            "SELECT * FROM user",
            "SELECT * FROM message",
            // It is legal to have redundant subscriptions.
            // However, keep in mind that data will be sent over the wire multiple times,
            // once for each subscriptions. This can cause slowdowns if you aren't careful.
            "SELECT * FROM message" });

    // You can also use SubscribeToAllTables, but it should be avoided if you have any large tables:
    // conn.SubscriptionBuilder().OnApplied(OnSubscriptionApplied).SubscribeToAllTables();

}

void OnConnectError(Exception e)
{

}

void OnDisconnect(DbConnection conn, Exception? e)
{

}

void PrintMessagesInOrder(RemoteTables tables)
{
    foreach (Message message in tables.Message.Iter().OrderBy(item => item.Sent))
    {
        PrintMessage(tables, message);
    }
}

void OnSubscriptionApplied(SubscriptionEventContext ctx)
{
    Console.WriteLine("Connected");
    PrintMessagesInOrder(ctx.Db);
}

void ProcessThread(DbConnection conn, CancellationToken ct)
{
    try
    {
        // loop until cancellation token
        while (!ct.IsCancellationRequested)
        {
            conn.FrameTick();

            ProcessCommands(conn.Reducers);

            Thread.Sleep(100);
        }
    }
    finally
    {
        conn.Disconnect();
    }
}

void InputLoop()
{
    while (true)
    {
        var input = Console.ReadLine();
        if (input == null)
        {
            break;
        }

        if (input.StartsWith("/name "))
        {
            input_queue.Enqueue(("name", input[6..]));
            continue;
        }
        else
        {
            input_queue.Enqueue(("message", input));
        }
    }
}

void ProcessCommands(RemoteReducers reducers)
{
    // process input queue commands
    while (input_queue.TryDequeue(out var command))
    {
        switch (command.Command)
        {
            case "message":
                reducers.SendMessage(command.Args);
                break;
            case "name":
                reducers.SetName(command.Args);
                break;
        }
    }
}

Main();
