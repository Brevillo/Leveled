/* Made by Oliver Beebe 2023 */
using System.Collections.Generic;
using System;

namespace OliverBeebe.UnityUtilities.Runtime 
{
    [Serializable]
    public class StateMachine 
    {

        #if UNITY_EDITOR
        [UnityEngine.SerializeField, UnityEngine.TextArea(3, 3)] private string debug;
        #endif

        public event Action<IState, IState> OnTransition;

        private readonly IState firstState;
        private readonly TransitionDictionary transitions;
        
        private IState currentState; 
        private IState previousState;
        private float stateDuration; 

        public IState CurrentState  => currentState;
        public IState PreviousState => previousState;
        public float StateDuration  => stateDuration;
        
        public StateMachine(IState firstState, TransitionDictionary transitions)
        {
            this.firstState = firstState;
            this.transitions = transitions;

            Reset();
        }

        public void Reset() 
        {
            currentState?.Exit();

            currentState = previousState = firstState;
            stateDuration = float.MaxValue;

            currentState.Enter();
        }

        public void ChangeState(IState toState) 
        {
            currentState?.Exit();

            previousState = currentState;
            currentState = toState;
            stateDuration = 0;

            currentState.Enter();
 
            OnTransition?.Invoke(previousState, currentState);
        }

        public void Update(float dt) 
        {
            stateDuration += dt;
            currentState.Update();

            if (transitions.TryGetValue(currentState, out var stateTransitions))
            {
                var transition = stateTransitions.Find(Transition.CanTransition);
                if (transition != null) ChangeState(transition);
            }

            #if UNITY_EDITOR
            debug = $"Current State : {currentState.GetType().Name}\nPrevious State : {previousState.GetType().Name}\nDuration : {stateDuration}";
            #endif
        }

        private void ChangeState(Transition transition)
        {
            currentState?.Exit();

            transition.InvokeTransitionBehavior();

            previousState = currentState;
            currentState = transition.ToState;
            stateDuration = 0;

            currentState.Enter();

            OnTransition?.Invoke(previousState, currentState);
        }
    }

    public class TransitionDictionary : Dictionary<IState, List<Transition>> { }

    public delegate bool TransitionDelegate();

    public class Transition
    {
        public static bool CanTransition(Transition transition) => transition.condition.Invoke();
        public IState ToState => toState;
        public void InvokeTransitionBehavior() => behavior?.Invoke();

        private readonly TransitionDelegate condition;
        private readonly IState toState;
        private readonly Action behavior;

        public Transition(IState toState, TransitionDelegate condition)
        {
            this.toState = toState;
            this.condition = condition;
            this.behavior = null;
        }

        public Transition(IState toState, TransitionDelegate condition, Action behavior)
        {
            this.toState = toState;
            this.condition = condition;
            this.behavior = behavior;
        }
    }

    public interface IState
    {
        public void Enter();
        public void Update();
        public void Exit();
    }

    public abstract class State<TContext> : IState 
    {
        protected State(TContext context) => this.context = context;

        protected readonly TContext context;

        public virtual void Enter () { }
        public virtual void Update() { }
        public virtual void Exit  () { }
    }

    public abstract class SubState<TContext, TSuperState> : State<TContext> where TSuperState : State<TContext> 
    {
        protected SubState(TContext context, TSuperState superState) : base(context) => this.superState = superState;

        protected readonly TSuperState superState;

        public override void Enter()
        {
            base.Enter();
            superState.Enter();
        }
        public override void Update() 
        {
            superState.Update();
            base.Update();
        }
        public override void Exit()  
        {
            superState.Exit();
            base.Exit();
        }
    }
}
