/* Made by Oliver Beebe 2025 */
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace OliverBeebe.UnityUtilities.Runtime
{
    [Serializable]
    public class StateMachine
    {
        #if UNITY_EDITOR
        [SerializeField, TextArea(3, 3)] private string debug;
        #endif

        public event Action<IStateBehavior, IStateBehavior> OnTransition;
        
        public State defaultState;
        public readonly List<State> stateGraph = new();

        private State currentState;
        private State previousState;
        private float stateDuration;

        public IStateBehavior CurrentState  => currentState.behavior;
        public IStateBehavior PreviousState => previousState.behavior;
        public float StateDuration  => stateDuration;
        
        public void Reset() 
        {
            currentState?.behavior.Exit();

            currentState = previousState = defaultState;
            stateDuration = float.MaxValue;

            currentState.behavior.Enter();
        }

        public void Update(float dt) 
        {
            if (currentState == null)
            {
                Reset();

                if (currentState == null) return;
            }
            
            stateDuration += dt;
            currentState.behavior.StateDuration = stateDuration;
            currentState.behavior.Update();
            
            var transition = currentState.transitions.Find(transition => transition.condition?.Invoke() ?? true);
            if (transition != null)
            {
                ChangeState(stateGraph.First(state => state.behavior.GetType() == transition.toState),
                    transition.behavior);
            }

            #if UNITY_EDITOR
            debug = $"Current State : {currentState.behavior.GetType().Name}\nPrevious State : {previousState.behavior.GetType().Name}\nDuration : {stateDuration}";
            #endif
        }

        public void ChangeState(State newState, Action transitionBehavior)
        {
            currentState?.behavior.Exit();

            transitionBehavior?.Invoke();
            
            previousState = currentState;
            currentState = newState;
            stateDuration = 0f;
            
            currentState.behavior.Enter();
            
            OnTransition?.Invoke(previousState?.behavior, currentState.behavior);
        }
    }

    public class State
    {
        public State(IStateBehavior behavior)
        {
            this.behavior = behavior;
            transitions = new();
        }
        
        public readonly IStateBehavior behavior;
        public readonly List<Transition> transitions;
    }

    public class Transition
    {
        public Transition(Type toState, TransitionCondition condition, Action behavior)
        {
            this.toState = toState;
            this.condition = condition;
            this.behavior = behavior;
        }
        
        public readonly TransitionCondition condition;
        public readonly Type toState;
        public readonly Action behavior;
    }

    public interface IStateBehavior
    {
        public void Enter();
        public void Update();
        public void Exit();
        
        public float StateDuration { get; set; }
    }

    public interface IContextStateBehavior<T> : IStateBehavior
    {
        public T Context { get; set; }
    }
    
    public delegate bool TransitionCondition();

    public static class StateMachineUtility
    {
        public static void ChangeState<T>(this StateMachine stateMachine)
            where T : IStateBehavior =>
            stateMachine.ChangeState(
                stateMachine.stateGraph.FirstOrDefault(state => state.behavior.GetType() == typeof(T)), null);

        public static State AddState<T>(this StateMachine stateMachine, T behavior)
            where T : IStateBehavior
        {
            var state = new State(behavior);
            stateMachine.stateGraph.Add(state);

            if (stateMachine.defaultState == null)
            {
                stateMachine.defaultState = state;
            }

            return state;
        }
        
        public static void SetDefaultState<T>(this StateMachine stateMachine)
            where T : IStateBehavior
        {
            stateMachine.defaultState = stateMachine.stateGraph.FirstOrDefault(state => state.behavior.GetType() == typeof(T));
        }
        
        public static void AddTransition<TFrom, TTo>(this StateMachine stateMachine, TransitionCondition condition) 
            where TFrom : IStateBehavior where TTo : IStateBehavior =>
            stateMachine.AddTransition<TFrom, TTo>(condition, null);
        
        public static void AddTransition<TFrom, TTo>(this StateMachine stateMachine, TransitionCondition condition, Action behavior)
            where TFrom : IStateBehavior where TTo : IStateBehavior 
        {
            var state = stateMachine.stateGraph.FirstOrDefault(state => state.behavior.GetType() == typeof(TFrom));

            if (state == null)
            {
                Debug.LogError($"StateMachine does not contain a state of type \"{typeof(TFrom).Name}\"");
                return;
            }
                
            state.transitions.Add(new(typeof(TTo), condition, behavior));
        }
        
        public static State AddTransition<TToState>(this State state, TransitionCondition condition)
            where TToState : IStateBehavior =>
            state.AddTransition<TToState>(condition, null);

        public static State AddTransition<TToState>(this State state, TransitionCondition condition, Action behavior)
            where TToState : IStateBehavior
        {
            state.transitions.Add(new(typeof(TToState), condition, behavior));

            return state;
        }
        
        public static void InitializeAllStatesWithContext<T>(this StateMachine stateMachine, T context)
        {
            foreach (var state in stateMachine.stateGraph)
            {
                if (state.behavior is IContextStateBehavior<T> behavior)
                {
                    behavior.Context = context;
                }
            }
        }
    }
}
