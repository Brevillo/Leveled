using System.Linq;
using UnityEngine;

public class TriggerTargeting : TargetingStrategy
{
    [SerializeField] private CollisionAggregate2D collisions;

    private void Update()
    {
        var validTargets = collisions.Colliders
            .Select(collider => collider.GetComponent<Targetable>())
            .Where(targetable => targetable != null && TargetableFilter(targetable))
            .ToList();
        
        if (!validTargets.Contains(activeTarget) || activeTarget == null)
        {
            activeTarget = validTargets.FirstOrDefault();
        }
    }
}