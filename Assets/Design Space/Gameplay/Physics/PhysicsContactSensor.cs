using System;
using UnityEngine;

public class PhysicsContactSensor : MonoBehaviour
{
    [SerializeField] private PhysicsContactChild child;

    private void OnTriggerEnter2D(Collider2D other) => TryAdd(other);
    private void OnTriggerExit2D(Collider2D other) => TryRemove(other);

    private void OnCollisionEnter2D(Collision2D other) => TryAdd(other.collider);
    private void OnCollisionExit2D(Collision2D other) => TryRemove(other.collider);

    private void TryRemove(Collider2D other)
    {
        if (!other.TryGetComponent(out PhysicsContactParent parent)) return;

        child.RemoveParent(parent);
    }

    private void TryAdd(Collider2D other)
    {
        if (!other.TryGetComponent(out PhysicsContactParent parent)) return;

        child.AddParent(parent);
    }
}