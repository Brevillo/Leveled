using System;
using OliverBeebe.UnityUtilities.Runtime;
using UnityEngine;
using StateMachine = OliverBeebe.UnityUtilities.Runtime.StateMachine;

public class Thwomp : MonoBehaviour
{
    [SerializeField] private TargetingStrategy targetingStrategy;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private CollisionAggregate2D groundCheck;
    [Header("Dropping")]
    [SerializeField] private float dropDelay;
    [SerializeField] private float dropAcceleration;
    [SerializeField] private float maxDropSpeed;
    [Header("Rising")]
    [SerializeField] private float riseDelay;
    [SerializeField] private float riseAcceleration;
    [SerializeField] private float maxRiseSpeed;

    private StateMachine stateMachine;

    private float startHeight;
    
    private void Awake()
    {
        InitStateMachine();
    }

    private void Start()
    {
        startHeight = transform.position.y;
    }

    private void InitStateMachine()
    {
        stateMachine = new();

        stateMachine.AddState<Idle>(new())
            .AddTransition<Dropping>(() => targetingStrategy.ActiveTarget != null);

        stateMachine.AddState<Dropping>(new())
            .AddTransition<Rising>(() => groundCheck.Touching);
        
        stateMachine.AddState<Rising>(new())
            .AddTransition<Idle>(() => transform.position.y >= startHeight);
        
        stateMachine.InitializeAllStatesWithContext(this);
        
        stateMachine.SetDefaultState<Idle>();
    }
    
    private void Update()
    {
        stateMachine.Update(Time.deltaTime);
    }
    
    private class Idle : ContextStateBehavior<Thwomp>
    {
        public override void Enter()
        {
            base.Enter();

            context.rigidbody.linearVelocity = Vector2.zero;
        }
    }

    private class Dropping : ContextStateBehavior<Thwomp>
    {
        private float delayTimer;
        
        public override void Enter()
        {
            base.Enter();
            
            delayTimer = 0f;
        }

        public override void Update()
        {
            delayTimer += Time.deltaTime;

            if (delayTimer < context.dropDelay)
            {
                return;
            }

            context.rigidbody.linearVelocityY = Mathf.MoveTowards(
                context.rigidbody.linearVelocityY,
                -context.maxDropSpeed, 
                context.dropAcceleration * Time.deltaTime);
            
            base.Update();
        }
    }

    private class Rising : ContextStateBehavior<Thwomp>
    {
        private float delayTimer;
        
        public override void Enter()
        {
            base.Enter();
            
            delayTimer = 0f;
        }

        public override void Update()
        {
            delayTimer += Time.deltaTime;

            if (delayTimer < context.dropDelay)
            {
                return;
            }

            context.rigidbody.linearVelocityY = Mathf.MoveTowards(
                context.rigidbody.linearVelocityY,
                context.maxRiseSpeed, 
                context.riseAcceleration * Time.deltaTime);
            
            base.Update();
        }
    }
}
