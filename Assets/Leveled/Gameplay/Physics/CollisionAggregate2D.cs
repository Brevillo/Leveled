using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime;

public class CollisionAggregate2D : MonoBehaviour
{
    public bool Touching => touching;

    [Readonly, SerializeField] private bool touching;
    private List<Collider2D> colliders;

    public List<Collider2D> Colliders
    {
        get
        {
            colliders.RemoveAll(collider => collider == null);
            return colliders;
        }
    }

    private void Awake()
    {
        colliders = new();
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!colliders.Contains(other))
        {
            colliders.Add(other);
        }
    }

    private void FixedUpdate()
    {
        touching = colliders.Count > 0;

        colliders.Clear();
    }
}
