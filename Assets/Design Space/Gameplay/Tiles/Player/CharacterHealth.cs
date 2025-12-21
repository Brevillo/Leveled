using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHealth : MonoBehaviour
{
    [SerializeField] private int maxHitpoints;
    [SerializeField] private int startingHitpoints;
    [SerializeField] private float invincibilityDuration;
    [SerializeField] private Damageable damageable;
    [SerializeField] private LevelResettable levelResettable;
    [SerializeField] private List<DamageType> instantKillDamageTypes;
    [SerializeField] private UnityEvent killed;
    
    public event Action Killed;
    
    private int hitpoints;
    private float invincibilityExpiration;

    public bool Invincible => Time.time < invincibilityExpiration;
    public int Hitpoints => hitpoints;
    public int HitpointsPercent => hitpoints / maxHitpoints;

    private void Awake()
    {
        damageable.Damaged += OnDamaged;

        if (levelResettable != null)
        {
            levelResettable.Reset += ResetHitpoints;
        }
    }
        
    private void Start()
    {
        ResetHitpoints();
    }
    
    private void OnDamaged(Damage damage)
    {
        if (hitpoints == 0 || Invincible) return;
        
        if (instantKillDamageTypes.Intersect(damage.damageTypes).Any())
        {
            hitpoints = 0;
        }
        else
        {
            hitpoints = (int)Mathf.MoveTowards(hitpoints, 0, 1);
        }
        
        if (hitpoints == 0)
        {
            Kill();
        }
        else
        {
            invincibilityExpiration = Time.time + invincibilityDuration;
        }
    }

    public void Heal(int healAmount)
    {
        hitpoints = (int)Mathf.MoveTowards(hitpoints, maxHitpoints, healAmount);
    }
    
    public void FullHeal()
    {
        hitpoints = maxHitpoints;
    }
    
    public void ResetHitpoints()
    {
        hitpoints = startingHitpoints;
    }
    
    public void Kill()
    {
        killed.Invoke();
        Killed?.Invoke();
    }
}
