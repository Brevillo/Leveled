using System;
using OliverBeebe.UnityUtilities.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
    [SerializeField] private int maxExtraJumps;
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

    [Header("Wall sliding")] 
    [SerializeField] private float startWallSlideSpeed;
    [SerializeField] private float maxWallSlideSpeed;
    [SerializeField] private float wallSlideAccel;
    
    [Header("References")] 
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private PositionRecorder positionRecorder;
    [SerializeField] private Bounceable bounceable;

    private Vector2 spawnPoint;
    private Checkpoint checkpoint;

    private int extraJumpsUsed;
    
    private Grounded grounded;
    private Jumping jumping;
    private Falling falling;
    private WallJumping wallJumping;
    private WallSliding wallSliding;
    private DoubleJumping doubleJumping;
    
    private StateMachine stateMachine;

    private int WallDirection 
        => rightWall.Touching ? 1
        : leftWall.Touching ? -1
        : 0;
    
    private Vector2 MoveInput
    {
        get
        {
            Vector2 input = moveInput.action.ReadValue<Vector2>();
            input.x = Sign0(input.x);
            input.y = Sign0(input.y);
            return input;
        }
    }

    private static float Sign0(float f) => f > 0 ? 1 : f < 0 ? -1 : 0;
    
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
        extraJumpsUsed = 0;
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
        wallJumping = new(this);
        wallSliding = new(this);
        doubleJumping = new(this);

        TransitionDelegate

            canGround = () => ground.Touching,

            canJump = () => jumpBuffer && ground.Touching,
            canCoyoteJump = () => jumpBuffer && stateMachine.PreviousState == grounded && stateMachine.StateDuration < coyoteTime,
            canDoubleJump = () => jumpBuffer && extraJumpsUsed < maxExtraJumps,
            canEndJump = () => rigidbody.linearVelocityY <= 0,

            canWalljump = () => WallDirection != 0 && jumpBuffer,
            canEndWalljump = () => stateMachine.StateDuration > walljumpNoTurnDuration
                                   || rigidbody.linearVelocityY <= 0,

            canWallSlide = () => WallDirection != 0 && WallDirection == MoveInput.x && !ground.Touching,
            canEndWallSlide = () => WallDirection == 0 || Sign0(MoveInput.x) == -WallDirection,

            canFall = () => !ground.Touching;

        Transition toWalljump = new(wallJumping, canWalljump);
        
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
                
                { doubleJumping, new()
                {
                    new(falling, canEndJump),
                    toWalljump,
                }},
                
                { falling, new()
                {
                    new(jumping, canCoyoteJump),
                    new(grounded, canGround),
                    toWalljump,
                    new(wallSliding, canWallSlide),
                    new(doubleJumping, canDoubleJump),
                }},
                
                { wallJumping, new()
                {
                    new(grounded, canGround),
                    new(falling, canEndWalljump),
                }},
                
                { wallSliding, new()
                {
                    new(falling, canEndWallSlide),
                    toWalljump,
                    new(grounded, canGround),
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
        
        public State(PlayerMovement context) : base(context) { }

        protected void Run(float accel, float deccel) =>
            Run(accel, deccel, context.MoveInput.x);

        protected void Run(float accel, float deccel, float input) =>
            VelocityX = Mathf.MoveTowards(VelocityX, input * context.runSpeed,
                (input != 0 ? accel : deccel) * Time.deltaTime);

        protected void Fall(float gravity) => 
            VelocityY = Mathf.MoveTowards(VelocityY, -context.maxFallSpeed, gravity * Time.deltaTime);
    }
    
    private class Grounded : State
    {
        public Grounded(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            base.Enter();
            
            context.extraJumpsUsed = 0;
        }

        public override void Update()
        {
            Run(context.groundAccel, context.groundDeccel);
            
            base.Update();
        }
    }
    
    private class Jumping : State
    {
        public Jumping(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            base.Enter();
            
            context.rigidbody.linearVelocityY = Mathf.Sqrt(2f * context.jumpHeight * context.jumpGravity);
            
            context.jumpBuffer.Reset();
        }

        public override void Update()
        {
            float gravity = context.jumpInput.action.IsPressed()
                ? context.jumpGravity
                : context.peakingGravity;
            
            Run(context.airAccel, context.airDeccel);
            Fall(gravity);

            base.Update();
        }
    }

    private class DoubleJumping : Jumping
    {
        public DoubleJumping(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            base.Enter();
            
            context.extraJumpsUsed++;
            
            context.jumpBuffer.Reset();
        }
    }
    
    private class Falling : State
    {
        public Falling(PlayerMovement context) : base(context) { }

        public override void Update()
        {
            Run(context.airAccel, context.airDeccel);
            Fall(context.fallGravity);
            
            base.Update();
        }
    }
    
    private class WallJumping : State
    {
        public WallJumping(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            base.Enter();
            
            context.extraJumpsUsed = 0;

            context.rigidbody.linearVelocity = new Vector2
            {
                y = Mathf.Sqrt(2f * context.walljumpHeight * context.jumpGravity),
                x = context.walljumpBoost * -context.WallDirection,
            };
            
            context.jumpBuffer.Reset();
        }

        public override void Update()
        {
            float input = context.MoveInput.x;
            
            if (Sign0(input) == context.WallDirection)
            {
                input = 0;
            }
            
            Run(context.airAccel, context.airDeccel, input);

            float gravity = context.jumpInput.action.IsPressed()
                ? context.jumpGravity
                : context.peakingGravity;
            
            Fall(gravity);

            base.Update();
        }
    }
    
    private class WallSliding : State
    {
        public WallSliding(PlayerMovement context) : base(context) { }

        public override void Enter()
        {
            base.Enter();
            
            context.rigidbody.linearVelocityY = Mathf.Max(context.rigidbody.linearVelocityY, -context.startWallSlideSpeed);
        }

        public override void Update()
        {
            context.rigidbody.linearVelocityY = Mathf.MoveTowards(context.rigidbody.linearVelocityY,
                -context.maxWallSlideSpeed, context.wallSlideAccel * Time.deltaTime);

            base.Update();
        }
    }
    
    /*
    private class Template : State
    {
        public Template(PlayerMovement context) : base(context) { }
    }
    */
}
