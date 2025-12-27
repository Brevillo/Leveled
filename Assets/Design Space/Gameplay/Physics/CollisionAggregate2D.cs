using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class CollisionAggregate2D : MonoBehaviour
{
    [SerializeField] private GameObjectFilter collisionFilter;
    
    public bool Touching => touching;

    public bool TriggerTouching => triggerTouching;

    public bool CollisionTouching => collisionTouching;

    [Readonly, SerializeField] private bool touching, triggerTouching, collisionTouching;
    
    private List<Collider2D> colliders, triggerColliders, collisionColliders;

    public List<Collider2D> Colliders
    {
        get
        {
            colliders.RemoveAll(collider => collider == null);
            return colliders;
        }
    }

    public List<Collider2D> TriggerColliders => triggerColliders;

    public List<Collider2D> CollisionColliders => collisionColliders;

    private void Awake()
    {
        colliders = new();
        triggerColliders = new();
        collisionColliders = new();
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        CheckCollision(other, triggerColliders);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        CheckCollision(other.collider, collisionColliders);
    }

    private void CheckCollision(Collider2D collider, List<Collider2D> specialCollisionRecord)
    {
        if (!colliders.Contains(collider) && (collisionFilter == null || collisionFilter.Filter(collider.gameObject)))
        {
            colliders.Add(collider);
            specialCollisionRecord.Add(collider);
        }
    }

    private void FixedUpdate()
    {
        touching = colliders.Count > 0;
        colliders.Clear();

        triggerTouching = triggerColliders.Count > 0;
        triggerColliders.Clear();

        collisionTouching = collisionColliders.Count > 0;
        collisionColliders.Clear();
    }
}
