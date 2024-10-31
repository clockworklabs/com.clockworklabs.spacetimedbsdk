using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Random = UnityEngine.Random;
using Vector2 = SpacetimeDB.Types.Vector2;

public class PlayModeExampleTest
{
    // [UnityTest] - This won't work until we have reconnections
    public IEnumerator SimpleConnectionTest()
    {
        PlayerPrefs.DeleteAll();
        var connected = false;
        var conn = DbConnection.Builder().OnConnect((a, b, c) =>
        {
            connected = true;
        }).OnConnectError((_, _) =>
        {
            Debug.Assert(false, "Connection failed!");
        }).WithUri("http://127.0.0.1:3000")
            .WithModuleName("untitled-circle-game").Build();

        while (!connected)
        {
            conn.FrameTick();
            yield return null;
        }
    }

    UnityEngine.Vector2 ToVector2(Vector2 v)
    {
        return new UnityEngine.Vector2(v.X, v.Y);
    }

    [UnityTest]
    public IEnumerator CreatePlayerAndTestDecay()
    {
        var connected = false;
        GameManager.OnConnect += () =>
        {
            Debug.Log("Connected");
            connected = true;
        };

        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Scenes/SampleScene");
        while(UIUsernameChooser.instance == null)
            yield return null;
        var playerCreated = false;

        GameManager.OnConnect += () =>
        {
            Debug.Log("Connected");
            connected = true;
        };

        while (!connected) yield return null;

        GameManager.conn.Reducers.OnCreatePlayer += (_, _) =>
        {
            Debug.Log("Player created");
            playerCreated = true;
        };

        UIUsernameChooser.instance.usernameInputField.text = "Test " + Random.Range(100000, 999999);
        UIUsernameChooser.instance.PlayPressed();
        while (!playerCreated) yield return null;

        Debug.Assert(GameManager.localIdentity != default, "GameManager.localIdentity != default");
        var player = GameManager.conn.Db.Player.Identity.Find(GameManager.localIdentity);
        Debug.Assert(player != null, nameof(player) + " != null");
        var circle = GameManager.conn.Db.Circle.Iter().FirstOrDefault(a => a.PlayerId == player.PlayerId);

        var foodEaten = 0;
        GameManager.conn.Db.Food.OnDelete += (ctx, food) =>
        {
            foodEaten++;
            Debug.Log("Food eaten!");
        };

        // Standing still should decay a bit
        PlayerController.Local.EnableTestInput();
        while(foodEaten < 200)
        {
            Debug.Assert(circle != null, nameof(circle) + " != null");
            var ourEntity = GameManager.conn.Db.Entity.Id.Find(circle.EntityId);
            var toChosenFood = new UnityEngine.Vector2(1000, 0);
            uint chosenFoodId = 0;
            foreach (var food in GameManager.conn.Db.Food.Iter())
            {
                var thisFoodId = food.EntityId;
                var foodEntity = GameManager.conn.Db.Entity.Id.Find(thisFoodId);
                Debug.Assert(foodEntity != null, nameof(foodEntity) + " != null");
                Debug.Assert(ourEntity != null, nameof(ourEntity) + " != null");
                var foodEntityPosition = foodEntity.Position;
                var ourEntityPosition = ourEntity.Position;
                Debug.Assert(foodEntityPosition != null, nameof(foodEntityPosition) + " != null");
                Debug.Assert(ourEntityPosition != null, nameof(ourEntityPosition) + " != null");
                var toThisFood = ToVector2(foodEntity.Position) - ToVector2(ourEntity.Position);
                if (toThisFood.sqrMagnitude == 0.0f) continue;
                if (toChosenFood.sqrMagnitude > toThisFood.sqrMagnitude)
                {
                    chosenFoodId = thisFoodId;
                    toChosenFood = toThisFood;
                }
            }

            if (GameManager.conn.Db.Entity.Id.Find(chosenFoodId) != null)
            {
                var ourNewEntity = GameManager.conn.Db.Entity.Id.Find(circle.EntityId);
                var foodEntity = GameManager.conn.Db.Entity.Id.Find(chosenFoodId);
                Debug.Assert(foodEntity != null, nameof(foodEntity) + " != null");
                Debug.Assert(ourNewEntity != null, nameof(ourNewEntity) + " != null");
                var toThisFood = ToVector2(foodEntity.Position) - ToVector2(ourNewEntity.Position);
                PlayerController.Local.SetTestInput(toThisFood);

            }

            yield return null;
        }


        PlayerController.Local.SetTestInput(UnityEngine.Vector2.zero);
        Debug.Assert(circle != null, nameof(circle) + " != null");
        var massStart = GameManager.conn.Db.Entity.Id.Find(circle.EntityId)!.Mass;
        yield return new WaitForSeconds(10);
        var massEnd = GameManager.conn.Db.Entity.Id.Find(circle.EntityId)!.Mass;
        Debug.Assert(massEnd < massStart, "Mass should have decayed");
    }

    // [UnityTest] - This won't work until we have reconnections
    public IEnumerator OneOffTest1()
    {
        var connected = false;
        GameManager.OnConnect += () =>
        {
            Debug.Log("Connected");
            connected = true;
        };

        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Scenes/SampleScene");
        while (UIUsernameChooser.instance == null)
            yield return null;
        var playerCreated = false;

        GameManager.OnConnect += () =>
        {
            Debug.Log("Connected");
            connected = true;
        };

        while (!connected) yield return null;

        GameManager.conn.Reducers.OnCreatePlayer += (ctx, username) =>
        {
            Debug.Log("Player created");
            playerCreated = true;
        };

        var name = "Test " + Random.Range(100000, 999999);
        UIUsernameChooser.instance.usernameInputField.text = name;
        UIUsernameChooser.instance.PlayPressed();
        while (!playerCreated) yield return null;

        var task = GameManager.conn.OneOffQuery<Player>(
            $"SELECT * FROM player WHERE identity=0x{GameManager.localIdentity}");
        Task.Run(() => task.RunSynchronously());
        while (!task.IsCompleted) yield return null;
        var players = task.Result;
        Debug.Assert(players.Length == 1, "Should have found one player");
        Debug.Assert(players[0].Name == name, "Username should match");
        Debug.Log($"id: {players[0].PlayerId} Username: {players[0].Name}");
    }

    // [UnityTest] - This won't work until we have reconnections
    public IEnumerator OneOffTest2()
    {
        var connected = false;
        GameManager.OnConnect += () =>
        {
            Debug.Log("Connected");
            connected = true;
        };

        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Scenes/SampleScene");
        while (UIUsernameChooser.instance == null)
            yield return null;
        var playerCreated = false;

        GameManager.OnConnect += () =>
        {
            Debug.Log("Connected");
            connected = true;
        };

        while (!connected) yield return null;

        GameManager.conn.Reducers.OnCreatePlayer += (ctx, username) =>
        {
            Debug.Log("Player created");
            playerCreated = true;
        };

        var name = "Test " + Random.Range(100000, 999999);
        UIUsernameChooser.instance.usernameInputField.text = name;
        UIUsernameChooser.instance.PlayPressed();
        while (!playerCreated) yield return null;

        var task = GameManager.conn.Db.Player.RemoteQuery($"WHERE identity=0x{GameManager.localIdentity}");
        Task.Run(() => task.RunSynchronously());
        while (!task.IsCompleted) yield return null;
        var players = task.Result;
        Debug.Assert(players.Length == 1, "Should have found one player");
        Debug.Assert(players[0].Name == name, "Username should match");
        Debug.Log($"id: {players[0].PlayerId} Username: {players[0].Name}");
    }

    //[UnityTest]
    public IEnumerator ReconnectionViaReloadingScene()
    {
        var connected = false;
        var subscribed = false;
        GameManager.OnConnect += () =>
        {
            connected = true;

        };
        GameManager.OnSubscriptionApplied += () =>
        {
            subscribed = true;
        };

        Debug.Log("Initial scene load!");
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Scenes/SampleScene");
        while(UIUsernameChooser.instance == null)
            yield return null;
        var playerCreated = false;

        while (!connected) yield return null;

        GameManager.conn.Reducers.OnCreatePlayer += (_, _) =>
        {
            Debug.Log("Player created");
            playerCreated = true;
        };

        var username = "Test " + Random.Range(100000, 999999);
        UIUsernameChooser.instance.usernameInputField.text = username;
        UIUsernameChooser.instance.PlayPressed();
        while (!playerCreated) yield return null;

        Debug.Assert(GameManager.localIdentity != default, "GameManager.localIdentity != default");
        var player = GameManager.conn.Db.Player.Identity.Find(GameManager.localIdentity);
        Debug.Assert(player != null, nameof(player) + " != null");
        var circle = GameManager.conn.Db.Circle.Iter().FirstOrDefault(a => a.PlayerId == player.PlayerId);

        connected = false;
        subscribed = false;
        GameManager.instance.Disconnect();

        Debug.Log("Second scene load!");
        // Reload
        SceneManager.LoadScene("Scenes/SampleScene");

        while(!connected || !subscribed) yield return null;
        var newPlayer = GameManager.conn.Db.Player.Identity.Find(GameManager.localIdentity);
        Debug.Assert(player.PlayerId == newPlayer.PlayerId, "PlayerIds should match!");
        var newCircle = GameManager.conn.Db.Circle.Iter().FirstOrDefault(a => a.PlayerId == newPlayer.PlayerId);
        Debug.Assert(circle.EntityId == newCircle.EntityId, "Circle EntityIds should match!");
    }
}
