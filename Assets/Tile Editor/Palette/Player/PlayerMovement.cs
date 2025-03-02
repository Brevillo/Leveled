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

    [Header("Walljumping")]
    [SerializeField] private float walljumpHeight;
    [SerializeField] private float walljumpBoost;
    [SerializeField] private float walljumpNoTurnDuration;
    [SerializeField] private CollisionAggregate2D leftWall;
    [SerializeField] private CollisionAggregate2D rightWall;
    
    [Header("References")] 
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private PositionRecorder positionRecorder;
    [SerializeField] private Bounceable bounceable;

    private Vector2 spawnPoint;
    private Checkpoint checkpoint;
    
    private Grounded grounded;
    private Jumping jumping;
    private Falling falling;
    private Walljumping walljumping;
    
    private StateMachine stateMachine;

    private int WallDirection 
        => rightWall.Touching ? 1
        : leftWall.Touching ? -1
        : 0;

    public void Respawn()
    {
        positionRecorder.AddPosition();

        rigidbody.interpolation = RigidbodyInterpolation2D.None;
        transform.position = checkpoint != null 
            ? checkpoint.transform.position
            : spawnPoint;
        rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        rigidbody.linearVelocity = Vector2.zero;
     
        positionRecorder.NewSegment();
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
        
        bounceable.Bounced += OnBounced;
    }

    private void OnBounced(Vector2 force)
    {
        stateMachine.ChangeState(falling);
        rigidbody.linearVelocity = force;
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
        walljumping = new(this);

        TransitionDelegate

            canGround = () => ground.Touching,
            
            canJump = () => ground.Touching && jumpBuffer,
            canEndJump = () => rigidbody.linearVelocityY <= 0,
            canCoyoteJump = () => stateMachine.previousState == grounded 
                                  && stateMachine.stateDuration < coyoteTime && jumpBuffer,
            
            canWalljump = () => WallDirection != 0 && jumpBuffer,
            canEndWalljump = () => stateMachine.stateDuration > walljumpNoTurnDuration 
                                   || rigidbody.linearVelocityY <= 0,
            
            canFall = () => !ground.Touching;

        Transition toWalljump = new(walljumping, canWalljump);
        
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
                    toWalljump,
                }},
                
                { falling, new()
                {
                    new(jumping, canCoyoteJump),
                    new(grounded, canGround),
                    toWalljump,
                }},
                
                { walljumping, new()
                {
                    new(grounded, canGround),
                    new(falling, canEndWalljump),
                }},
            });
    }

    private class State : State<PlayerMovement>
    {
        protected float VelocityX
        {
            get => context.rigidbody.linearVelocityX;
            set => context.rigidbody.linearVelocityX = value;
        }
        
        protected float VelocityY
        {
            get => context.rigidbody.linearVelocityY;
            set => context.rigidbody.linearVelocityY = value;
        }

        protected float InputX => context.moveInput.action.ReadValue<Vector2>().x;
        
        public State(PlayerMovement context) : base(context) { }

        protected void Run(float accel, float deccel) =>
            Run(accel, deccel, InputX);

        protected void Run(float accel, float deccel, float input) =>
            VelocityX = Mathf.MoveTowards(VelocityX, input * context.runSpeed,
                (input != 0 ? accel : deccel) * Time.deltaTime);

        protected void Fall(float gravity) => 
            VelocityY = Mathf.MoveTowards(VelocityY, -context.maxFallSpeed, gravity * Time.deltaTime);

        protected static float Sign0(float f) => f > 0 ? 1 : f < 0 ? -1 : 0;
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
            context.rigidbody.linearVelocityY = Mathf.Sqrt(2f * context.jumpHeight * context.jumpGravity);
            
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
    
    private class Walljumping : State
    {
        public Walljumping(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            context.rigidbody.linearVelocity = new Vector2
            {
                y = Mathf.Sqrt(2f * context.walljumpHeight * context.jumpGravity),
                x = context.walljumpBoost * -context.WallDirection,
            };
            
            context.jumpBuffer.Reset();
            
            base.Enter();
        }

        public override void Update()
        {
            base.Update();

            float input = InputX;
            
            if (Sign0(input) == context.WallDirection)
            {
                input = 0;
            }
            
            Run(context.airAccel, context.airDeccel, input);

            float gravity = context.jumpInput.action.IsPressed()
                ? context.jumpGravity
                : context.peakingGravity;
            
            Fall(gravity);
        }
    }
    
    /*
    private class Template : State
    {
        public Template(PlayerMovement context) : base(context) { }
    }
    */
}
