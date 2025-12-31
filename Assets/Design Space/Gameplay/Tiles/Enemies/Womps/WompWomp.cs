using System;
using OliverBeebe.UnityUtilities.Runtime;
using UnityEngine;
using UnityEngine.Events;
using StateMachine = OliverBeebe.UnityUtilities.Runtime.StateMachine;

public class WompWomp : MonoBehaviour
{
    [SerializeField] private TargetingStrategy targetingStrategy;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private CollisionAggregate2D groundCheck;
    [SerializeField] private CollisionAggregate2D ceilingCheck;
    [Header("Dropping")]
    [SerializeField] private float dropDelay;
    [SerializeField] private float dropAcceleration;
    [SerializeField] private float maxDropSpeed;
    [SerializeField] private UnityEvent onDrop;
    [Header("Rising")]
    [SerializeField] private float riseDelay;
    [SerializeField] private float riseAcceleration;
    [SerializeField] private float maxRiseSpeed;
    [Header("Grounded")]
    [SerializeField] private UnityEvent onGrounded;

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
            .AddTransition<Dropping>(() => targetingStrategy.activeTarget != null);

        stateMachine.AddState<Dropping>(new())
            .AddTransition<Grounded>(() => groundCheck.Touching);

        stateMachine.AddState<Grounded>(new())
            .AddTransition<Rising>(() => stateMachine.StateDuration > riseDelay);
        
        stateMachine.AddState<Rising>(new())
            .AddTransition<Idle>(() => transform.position.y >= startHeight, () => rigidbody.MovePosition(new(rigidbody.position.x, startHeight)))
            .AddTransition<Idle>(() => ceilingCheck.Touching);
        
        stateMachine.InitializeAllStatesWithContext(this);
    }
    
    private void Update()
    {
        stateMachine.Update(Time.deltaTime);
    }

    private class Idle : ContextStateBehavior<WompWomp>
    {
        public override void Enter()
        {
            Context.rigidbody.linearVelocity = Vector2.zero;
        }
    }

    private class Dropping : ContextStateBehavior<WompWomp>
    {
        public override void Update()
        {
            if (StateDuration < Context.dropDelay)
            {
                return;
            }

            Context.rigidbody.linearVelocityY = Mathf.MoveTowards(
                Context.rigidbody.linearVelocityY,
                -Context.maxDropSpeed, 
                Context.dropAcceleration * Time.deltaTime);
        }

        public override void Enter()
        {
            Context.onDrop.Invoke();
        }
    }

    private class Grounded : ContextStateBehavior<WompWomp>
    {
        public override void Enter()
        {
            Context.onGrounded.Invoke();
        }
    }
    
    private class Rising : ContextStateBehavior<WompWomp>
    {
        public override void Update()
        {
            Context.rigidbody.linearVelocityY = Mathf.MoveTowards(
                Context.rigidbody.linearVelocityY,
                Context.maxRiseSpeed, 
                Context.riseAcceleration * Time.deltaTime);
        }
    }
}
