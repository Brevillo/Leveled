using System;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Damageable damageable))
        {
            damageable.TakeDamage();
        }
    }
}
