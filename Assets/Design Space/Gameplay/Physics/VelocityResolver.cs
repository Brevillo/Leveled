using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VelocityResolver : MonoBehaviour
{
    [SerializeField] private new Rigidbody2D rigidbody;
    
    private Dictionary<object, Vector2> registrants;

    private Vector2 CalculatedVelocity => registrants.Values.Aggregate(Vector2.zero, (sum, next) => sum + next);

    public Rigidbody2D Rigidbody => rigidbody;

    public Vector2 GetLiveVelocity(object key) =>
        rigidbody.linearVelocity - CalculatedVelocity + registrants.GetValueOrDefault(key);

    public void SetVelocity(object key, Vector2 velocity)
    {
        registrants[key] = velocity;
        
        UpdateVelocity();
    }

    public void SetTotalVelocityY(object key, float velocity)
    {
        Vector2 current = registrants[key];
        current.y = velocity;
        
        Vector2 otherVelocity = registrants.Where(kv => kv.Key != key).Aggregate(Vector2.zero, (sum, next) => sum + next.Value);
        
        registrants[key] = current - otherVelocity;
        
        UpdateVelocity();
    }

    public void UnsetVelocity(object key)
    {
        registrants.Remove(key);
    }

    private void UpdateVelocity()
    {
        rigidbody.linearVelocity = CalculatedVelocity;
    }

    private void Awake()
    {
        registrants = new();
    }
}