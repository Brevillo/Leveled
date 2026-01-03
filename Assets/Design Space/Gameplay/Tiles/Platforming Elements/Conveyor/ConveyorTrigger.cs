using System;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorTrigger : MonoBehaviour
{
    [SerializeField] private Vector2 speed;

    private HashSet<VelocityResolver> activeResolvers;

    private void Awake()
    {
        activeResolvers = new();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out VelocityResolver velocityResolver))
        {
            velocityResolver.SetVelocity(this, speed);
            activeResolvers.Add(velocityResolver);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out VelocityResolver velocityResolver) && activeResolvers.Contains(velocityResolver))
        {
            velocityResolver.UnsetVelocity(this);
            activeResolvers.Remove(velocityResolver);
        }
    }
}
