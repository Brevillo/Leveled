using OliverBeebe.UnityUtilities.Runtime;
using UnityEngine;

public class Gnome : MonoBehaviour
{
    [SerializeField] private Transform hidingPosition;
    [SerializeField] private Transform attackingPosition;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private TargetingStrategy dontAttackTargeting;
    [SerializeField] private float hideDuration;
    [SerializeField] private float attackDuration;
    [SerializeField] private float transitionSpeed;
    
    private StateMachine stateMachine;

    private void Awake()
    {
        InitializeStateMachine();
        
        rigidbody.MovePosition(hidingPosition.position);
    }

    private void InitializeStateMachine()
    {
        stateMachine = new();

        stateMachine.AddState<Hidden>(new())
            .AddTransition<Attacking>(() => stateMachine.StateDuration > hideDuration &&
                                            (dontAttackTargeting == null || dontAttackTargeting.activeTarget == null));
        
        stateMachine.AddState<Attacking>(new())
            .AddTransition<Attack>(() => rigidbody.position == (Vector2)attackingPosition.position);

        stateMachine.AddState<Attack>(new())
            .AddTransition<Hiding>(() => stateMachine.StateDuration > attackDuration);

        stateMachine.AddState<Hiding>(new())
            .AddTransition<Hidden>(() => rigidbody.position == (Vector2)hidingPosition.position);
        
        stateMachine.InitializeAllStatesWithContext(this);
    }

    private void Update()
    {
        stateMachine.Update(Time.deltaTime);
    }
    
    private class State : IContextStateBehavior<Gnome>
    {
        public float StateDuration { get; set; }
        public Gnome Context { get; set; }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }

        protected void MoveTowards(Transform target) => Context.rigidbody.MovePosition(Vector2.MoveTowards(
            Context.rigidbody.position, 
            target.position,
            Context.transitionSpeed * Time.deltaTime));
    }

    private class Hidden : State { }
        
    private class Attacking : State
    {
        public override void Update()
        {
            MoveTowards(Context.attackingPosition);
        }
    }
    
    private class Attack : State { }
    
    private class Hiding : State
    {
        public override void Update()
        {
            MoveTowards(Context.hidingPosition);
        }
    }
}
