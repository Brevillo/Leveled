using System;
using UnityEngine;

public class Collector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Collectable collectable))
        {
            collectable.Collect();
        }
    }
}
