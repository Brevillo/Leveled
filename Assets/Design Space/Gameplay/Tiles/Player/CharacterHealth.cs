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
    public event Action HitpointsUpdated;
    
    private int hitpoints;
    private float invincibilityExpiration;
    private bool invincible;
    
    public bool Invincible
    {
        get => invincible || Time.time < invincibilityExpiration;
        set => invincible = value;
    }

    public int Hitpoints
    {
        get => hitpoints;
        set
        {
            if (hitpoints == value) return;

            hitpoints = value;
            
            HitpointsUpdated?.Invoke();
        }
    }

    public float HitpointsPercent => (float)Hitpoints / maxHitpoints;

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
        if (Hitpoints == 0 || Invincible) return;
        
        if (instantKillDamageTypes.Intersect(damage.damageTypes).Any())
        {
            Hitpoints = 0;
        }
        else
        {
            Hitpoints = (int)Mathf.MoveTowards(Hitpoints, 0, 1);
        }
        
        if (Hitpoints == 0)
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
        Hitpoints = (int)Mathf.MoveTowards(hitpoints, maxHitpoints, healAmount);
    }
    
    public void FullHeal()
    {
        Hitpoints = maxHitpoints;
    }
    
    public void ResetHitpoints()
    {
        Hitpoints = startingHitpoints;
    }
    
    public void Kill()
    {
        killed.Invoke();
        Killed?.Invoke();
    }
}
