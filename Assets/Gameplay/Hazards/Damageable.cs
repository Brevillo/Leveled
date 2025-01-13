using System;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [SerializeField] private UnityEvent damaged;
    public event Action Damaged;

    public void TakeDamage()
    {
        Damaged?.Invoke();
        damaged.Invoke();
    }
}
