using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

public class FoodController : MonoBehaviour
{
    [DoNotSerialize] public uint entityId;
    public Renderer rend;
    
    private static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");

    public void Spawn(uint entityId)
    {
        this.entityId = entityId;
        GameManager.conn.Db.Food.OnDelete += OnDelete;
        
        var entity = GameManager.conn.Db.Entity.Id.Find(entityId);
        var position = new UnityEngine.Vector2
        {
            x = entity.Position.X,
            y = entity.Position.Y,
        };
        var foodRadius = GameManager.MassToRadius(entity.Mass);
        transform.localScale = new Vector3
        {
            x = foodRadius * 2,
            y = foodRadius * 2,
            z = foodRadius * 2,
        };
        transform.position = position;
        rend.material.SetColor(MainTexProperty, GameManager.GetRandomColor(entity.Id));
    }

    private void OnDestroy()
    {
        if (GameManager.IsConnected())
        {
            GameManager.conn.Db.Food.OnDelete -= OnDelete;        
        }
    }

    private void OnDelete(EventContext context, Food food)
    {
        if (food.EntityId == entityId)
        {
            Destroy(gameObject);
        }
    }
}