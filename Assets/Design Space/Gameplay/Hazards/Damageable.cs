using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public readonly struct Damage
{
    public readonly List<DamageType> damageTypes;
    
    public Damage(List<DamageType> damageTypes)
    {
        this.damageTypes = damageTypes;
    }
}

public readonly struct DamageResult
{
    public readonly bool received;
    
    public DamageResult(bool received)
    {
        this.received = received;
    }
}

public class Damageable : MonoBehaviour
{
    public List<DamageType> acceptedDamageTypes;
    [SerializeField] private bool destroyOnDamaged;
    [SerializeField] private GameObject destroyTarget;
    [SerializeField] private UnityEvent damaged;
    public event Action Damaged;
    
    public DamageResult TakeDamage(Damage damage)
    {
        if (!acceptedDamageTypes.Exists(type => damage.damageTypes.Contains(type))) return default;
        
        Damaged?.Invoke();
        damaged.Invoke();

        if (destroyOnDamaged)
        {
            Destroy(destroyTarget);
        }

        return new(true);
    }
}
