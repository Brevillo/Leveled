using System;
using System.Linq;
using OliverBeebe.UnityUtilities.Runtime;
using UnityEngine;
using UnityEngine.Events;

public class DonutPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay;
    [SerializeField] private float fallSpeed;
    [SerializeField] private float respawnDelay;
    [SerializeField] private CollisionAggregate2D standingDetection;
    [SerializeField] private CollisionAggregate2D respawnCheck;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private GameObject content;
    [SerializeField] private TilePlacerReference tilePlacerReference;
    [SerializeField] private new Collider2D collider;
    [SerializeField] private PlatformEffector2D platformEffector;
    [Header("Visuals")] 
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite triggeredSprite;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Header("Audio")] 
    [SerializeField] private UnityEvent platformCrumble;

    private bool onStep;
    private float startHeight;

    private StateMachine stateMachine;

    private void Start()
    {
        stateMachine = new();

        stateMachine.AddState<Idle>(new());
        stateMachine.AddState<Falling>(new())
            .AddTransition<Respawning>(() => rigidbody.position.y < tilePlacerReference.value.Bounds.min.y);
        stateMachine.AddState<Respawning>(new())
            .AddTransition<Idle>(() => stateMachine.StateDuration > respawnDelay && !respawnCheck.CollisionTouching);
        
        stateMachine.InitializeAllStatesWithContext(this);
        
        startHeight = rigidbody.position.y;
    }

    private void Update()
    {
        stateMachine.Update(Time.deltaTime);
    }

    private class Idle : IContextStateBehavior<DonutPlatform>
    {
        public float StateDuration { get; set; }
        public DonutPlatform Context { get; set; }

        private float fallTimer;

        public void Enter()
        {
            fallTimer = 0f;
            
            Vector2 position = Context.rigidbody.position;
            position.y = Context.startHeight;
            Context.rigidbody.position = position;

            Context.rigidbody.linearVelocityY = 0f;

            Context.collider.enabled = false;
            Context.collider.enabled = true;
        }

        public void Update()
        {
            if (Context.standingDetection.Colliders
                .Any(collider => collider.TryGetComponent<HeavyObject>(out var heavy) && heavy.grounded))
            {
                fallTimer += Time.deltaTime;
                Context.spriteRenderer.sprite = Context.triggeredSprite;
                if (Context.onStep)
                {
                    Context.platformCrumble.Invoke();
                    Context.onStep = false;
                }
            }
            else
            {
                Context.spriteRenderer.sprite = Context.normalSprite;
                fallTimer = 0f;
                Context.onStep = true;
            }

            if (fallTimer >= Context.fallDelay)
            {
                Context.stateMachine.ChangeState<Falling>();
            }
        }

        public void Exit() { }
    }

    private class Falling : IContextStateBehavior<DonutPlatform>
    {
        public DonutPlatform Context { get; set; }
        public float StateDuration { get; set; }

        public void Enter()
        {
            Context.platformEffector.enabled = true;
        }
        
        public void Update()
        {
            Context.rigidbody.linearVelocityY = -Context.fallSpeed;
        }

        public void Exit()
        {
            Context.platformEffector.enabled = false;
        }
    }

    private class Respawning : IContextStateBehavior<DonutPlatform>
    {
        public float StateDuration { get; set; }
        public DonutPlatform Context { get; set; }

        public void Enter()
        {
            Context.content.SetActive(false);
        }

        public void Update() { }

        public void Exit()
        {
            Context.content.SetActive(true);
        }
    }
}
