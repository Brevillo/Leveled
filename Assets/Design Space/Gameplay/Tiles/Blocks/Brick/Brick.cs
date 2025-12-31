using System;
using System.Collections;
using UnityEngine;

public class Brick : MonoBehaviour
{
    [SerializeField] private Damageable damageable;
    [SerializeField] private float destructionDelay;

    private void Awake()
    {
        damageable.Damaged += OnDamaged;
    }

    private void OnDamaged(Damage damage)
    {
        StartCoroutine(Destruction());
    }

    private IEnumerator Destruction()
    {
        yield return new WaitForSeconds(destructionDelay);
        
        Destroy(gameObject);
    } 
}
