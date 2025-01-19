using System;
using OliverBeebe.UnityUtilities.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Running")]
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private float runSpeed;
    [SerializeField] private float groundAccel;
    [SerializeField] private float groundDeccel;
    [SerializeField] private float airAccel;
    [SerializeField] private float airDeccel;

    [Header("Jumping")] 
    [SerializeField] private InputActionReference jumpInput;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpGravity;
    [SerializeField] private float peakingGravity;
    [SerializeField] private float fallGravity;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float coyoteTime;
    [SerializeField] private BufferTimer jumpBuffer;
    [SerializeField] private CollisionAggregate2D ground;
    
    [Header("References")] 
    [SerializeField] private new Rigidbody2D rigidbody;

    private Vector2 spawnPoint;
    private Checkpoint checkpoint;
    
    private Grounded grounded;
    private Jumping jumping;
    private Falling falling;
    private StateMachine stateMachine;

    public void Respawn()
    {
        rigidbody.position = checkpoint != null 
            ? checkpoint.transform.position
            : spawnPoint;
        
        rigidbody.linearVelocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Checkpoint newCheckpoint)
            && checkpoint != newCheckpoint)
        {
            if (checkpoint != null)
            {
                checkpoint.Deactivate();
            }

            checkpoint = newCheckpoint;
            
            checkpoint.Activate();
        }
    }

    private void Awake()
    {
        InitStateMachine();

        spawnPoint = transform.position;
    }

    private void Update()
    {
        jumpBuffer.Buffer(jumpInput.action.WasPerformedThisFrame());
        
        stateMachine.Update(Time.deltaTime);
    }

    private void InitStateMachine()
    {
        grounded = new(this);
        jumping = new(this);
        falling = new(this);

        TransitionDelegate

            canGround = () => ground.Touching,
            
            canJump = () => ground.Touching && jumpBuffer,
            canEndJump = () => rigidbody.linearVelocity.y <= 0,
            canCoyoteJump = () => stateMachine.previousState == grounded && stateMachine.stateDuration < coyoteTime && jumpBuffer,
            
            canFall = () => !ground.Touching;
        
        stateMachine = new(
            firstState: grounded,
            new()
            {
                { grounded, new()
                {
                    new(jumping, canJump),
                    new(falling, canFall),
                }},
                
                { jumping, new()
                {
                    new(falling, canEndJump),
                }},
                
                { falling, new()
                {
                    new(jumping, canCoyoteJump),
                    new(grounded, canGround),
                }}
            });
    }

    private class State : State<PlayerMovement>
    {
        public State(PlayerMovement context) : base(context) { }

        protected void Run(float accel, float deccel)
        {
            Vector2 velocity = context.rigidbody.linearVelocity;
            
            float input = context.moveInput.action.ReadValue<Vector2>().x;
            float currentAccel = input != 0 ? accel : deccel;

            velocity.x = Mathf.MoveTowards(velocity.x, input * context.runSpeed, currentAccel * Time.deltaTime);

            context.rigidbody.linearVelocity = velocity;
        }

        protected void Fall(float gravity)
        {
            Vector2 velocity = context.rigidbody.linearVelocity;

            velocity.y = Mathf.MoveTowards(velocity.y, -context.maxFallSpeed, gravity * Time.deltaTime);

            context.rigidbody.linearVelocity = velocity;
        }
    }
    
    private class Grounded : State
    {
        public Grounded(PlayerMovement context) : base(context) { }

        public override void Update()
        {
            base.Update();
            
            Run(context.groundAccel, context.groundDeccel);
        }
    }
    
    private class Jumping : State
    {
        public Jumping(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            Vector2 velocity = context.rigidbody.linearVelocity;
            velocity.y = Mathf.Sqrt(2f * context.jumpHeight * context.jumpGravity);
            context.rigidbody.linearVelocity = velocity;
            
            context.jumpBuffer.Reset();
            
            base.Enter();
        }

        public override void Update()
        {
            base.Update();

            float gravity = context.jumpInput.action.IsPressed()
                ? context.jumpGravity
                : context.peakingGravity;
            
            Run(context.airAccel, context.airDeccel);
            Fall(gravity);
        }
    }
    
    private class Falling : State
    {
        public Falling(PlayerMovement context) : base(context) { }

        public override void Update()
        {
            base.Update();
            
            Run(context.airAccel, context.airDeccel);
            Fall(context.fallGravity);
        }
    }
    
    /*
    private class Template : State
    {
        public Template(PlayerMovement context) : base(context) { }
    }
    */
}
