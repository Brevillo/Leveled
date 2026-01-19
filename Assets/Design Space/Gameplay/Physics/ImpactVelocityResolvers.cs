using System.Collections.Generic;
using UnityEngine;

public class ImpactVelocityResolvers : MonoBehaviour
{
    [SerializeField] private new Rigidbody2D rigidbody;

    private readonly HashSet<VelocityResolver> activeResolvers = new();
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.TryGetComponent(out VelocityResolver velocityResolver))
        {
            activeResolvers.Add(velocityResolver);
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.TryGetComponent(out VelocityResolver velocityResolver))
        {
            velocityResolver.UnsetVelocity(this);
            activeResolvers.Remove(velocityResolver);
        }
    }

    private void Update()
    {
        foreach (var resolver in activeResolvers)
        {
            resolver.SetVelocity(this, rigidbody.linearVelocity);
        }
    }
}