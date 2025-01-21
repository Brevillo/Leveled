using System;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] private float force;
    [SerializeField] private Vector2 direction;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Bounceable bounceable))
        {
            bounceable.Bounce(direction * force);
        }
    }
}
