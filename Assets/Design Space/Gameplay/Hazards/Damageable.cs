using System;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [SerializeField] private UnityEvent damaged;
    public event Action Damaged;

    public bool invincible;
    
    public void TakeDamage()
    {
        if (invincible) return;
        
        Damaged?.Invoke();
        damaged.Invoke();
    }
}
