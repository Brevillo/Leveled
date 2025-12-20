using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageSource : MonoBehaviour
{
    [SerializeField] private List<DamageType> damageTypes;
    [SerializeField] private UnityEvent damageDealt;
    public event Action DamageDealt;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckForDamage(other);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        CheckForDamage(other.collider);
    }

    private void CheckForDamage(Collider2D other)
    {
        if (!enabled) return;

        if (!other.TryGetComponent(out Damageable damageable)) return;
        
        var result = damageable.TakeDamage(new(damageTypes));

        if (result.received)
        {
            DamageDealt?.Invoke();
            damageDealt.Invoke();
        }
    }
}
