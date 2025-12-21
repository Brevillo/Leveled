using System;
using OliverBeebe.UnityUtilities.Runtime;
using UnityEngine;
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
            .AddTransition<Dropping>(() => targetingStrategy.activeTarget != null);

        stateMachine.AddState<Dropping>(new())
            .AddTransition<Rising>(() => stateMachine.StateDuration > dropDelay && groundCheck.Touching);
        
        stateMachine.AddState<Rising>(new())
            .AddTransition<Idle>(() => stateMachine.StateDuration > riseDelay && (transform.position.y >= startHeight || ceilingCheck.Touching));
        
        stateMachine.InitializeAllStatesWithContext(this);
    }
    
    private void Update()
    {
        stateMachine.Update(Time.deltaTime);
    }
    
    private class Idle : IContextStateBehavior<WompWomp>
    {
        public float StateDuration { get; set; }
        public WompWomp Context { get; set; }

        public void Enter()
        {
            Context.rigidbody.linearVelocity = Vector2.zero;
        }

        public void Update() { }

        public void Exit() { }
    }

    private class Dropping : IContextStateBehavior<WompWomp>
    {
        public float StateDuration { get; set; }
        public WompWomp Context { get; set; }

        public void Enter() { }

        public void Update()
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

        public void Exit() { }
    }

    private class Rising : IContextStateBehavior<WompWomp>
    {
        public float StateDuration { get; set; }
        public WompWomp Context { get; set; }
        
        public void Enter() { }

        public void Update()
        {
            if (StateDuration < Context.riseDelay)
            {
                return;
            }

            Context.rigidbody.linearVelocityY = Mathf.MoveTowards(
                Context.rigidbody.linearVelocityY,
                Context.maxRiseSpeed, 
                Context.riseAcceleration * Time.deltaTime);
        }

        public void Exit() { }
    }
}
