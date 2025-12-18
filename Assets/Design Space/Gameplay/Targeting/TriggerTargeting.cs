using System.Linq;
using UnityEngine;

public class TriggerTargeting : TargetingStrategy
{
    [SerializeField] private CollisionAggregate2D collisions;

    private void Update()
    {
        activeTarget = collisions.Colliders
            .Select(collider => collider.GetComponent<Targetable>())
            .FirstOrDefault(targetable => targetable != null && TargetableFilter(targetable));
    }
}