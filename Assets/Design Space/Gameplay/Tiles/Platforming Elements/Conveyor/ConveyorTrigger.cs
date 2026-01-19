using System;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorTrigger : MonoBehaviour
{
    [SerializeField] private Vector2 speed;

    private HashSet<Transform> activeResolvers;

    private void Awake()
    {
        activeResolvers = new();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerMovement player))
        {
            activeResolvers.Add(player.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerMovement player) && activeResolvers.Contains(player.transform))
        {
            activeResolvers.Remove(player.transform);
        }
    }

    private void FixedUpdate()
    {
        foreach (var resolver in activeResolvers)
        {
            resolver.position += (Vector3)speed * Time.deltaTime;
        }
    }
}
