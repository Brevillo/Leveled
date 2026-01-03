using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Helmet : MonoBehaviour
{
    [SerializeField] private Damageable damageable;
    [SerializeField] private DamageSource damageSource; 
    [SerializeField] private BasicRunAndJumpMovement movement;
    [SerializeField] private Bounceable bounceable;
    [SerializeField] private List<DamageType> instantKillDamageTypes;
    [SerializeField] private new Rigidbody2D rigidbody;
    
    private State state;
    
    private enum State
    {
        Idle,
        Moving,
    }

    private void ChangeState(State newState)
    {
        state = newState;

        switch (newState)
        {
            case State.Idle:
                movement.running = false;
                rigidbody.linearVelocity = Vector2.zero;
                damageSource.enabled = false;
                break;
            
            case State.Moving:
                movement.running = true;
                damageSource.enabled = true;
                break;
        }
    }
    
    private void Awake()
    {
        bounceable.Bounced += OnBounced;
        damageable.Damaged += OnDamaged;
    }

    private void Start()
    {
        ChangeState(State.Idle);
    }

    private void OnDamaged(Damage damage)
    {
        if (instantKillDamageTypes.Intersect(damage.damageTypes).Any())
        {
            Destroy(gameObject);
            return;
        }
        
        ChangeState(State.Idle);
    }

    private void OnBounced(BounceParams bounceParams)
    {
        ChangeState(State.Moving);
    }
}
