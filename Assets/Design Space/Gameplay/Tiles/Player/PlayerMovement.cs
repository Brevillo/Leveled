using OliverBeebe.UnityUtilities.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Parameters
    
    [SerializeField] private float respawnIgnoreInputDuration;
    
    [Header("Running")]
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private float runSpeed;
    [SerializeField] private float groundAccel;
    [SerializeField] private float groundDeccel;
    [SerializeField] private float airAccel;
    [SerializeField] private float airDeccel;
    [SerializeField] private float groundDownForce;
    [SerializeField] private PhysicsContactSensor groundSensor;

    [Header("Jumping")] 
    [SerializeField] private InputActionReference jumpInput;
    [SerializeField] private int maxExtraJumps;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpGravity;
    [SerializeField] private float peakingGravity;
    [SerializeField] private float fallGravity;
    [SerializeField] private float peakAirControlMultiplier;
    [SerializeField] private float peakAirControlThreshold;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpBufferDuration;
    [SerializeField] private BoxCaster2D ground;
    [SerializeField] private SoundEffect jumpSound;
    [SerializeField] private DamageSource jumpDamageSource;
    [SerializeField] private DamageSource fallDamageSource;

    [Header("Bouncing")]
    [SerializeField] private float minBounceDuration;
    
    [Header("Walljumping")]
    [SerializeField] private bool walljumpingEnabled;
    [SerializeField] private float walljumpHeight;
    [SerializeField] private float walljumpBoost;
    [SerializeField] private float walljumpNoTurnDuration;
    [SerializeField] private CollisionAggregate2D leftWall;
    [SerializeField] private CollisionAggregate2D rightWall;

    [Header("Wall sliding")] 
    [SerializeField] private bool wallSlidingEnabled;
    [SerializeField] private float startWallSlideSpeed;
    [SerializeField] private float maxWallSlideSpeed;
    [SerializeField] private float wallSlideAccel;

    [SerializeField] private float noClipSpeed;
    [SerializeField] private float noClipAcceleration;
    [SerializeField] private float noClipDecceleration;
    
    [Header("References")] 
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private new BoxCollider2D collider;
    [SerializeField] private PositionRecorder positionRecorder;
    [SerializeField] private Bounceable bounceable;
    [SerializeField] private HeavyObject heavyObject;
    [SerializeField] private LevelResettable levelResettable;
    [SerializeField] private CheckpointEnjoyer checkpointEnjoyer;
    
    #endregion
    
    private Vector2 spawnPoint;

    private float respawnTime;
    private int extraJumpsUsed;

    private BufferTimer jumpBuffer;
    
    private StateMachine stateMachine;

    private bool immobilized;
    
    public void Immobilize(bool immobilized)
    {
        this.immobilized = immobilized;
        
        stateMachine.Reset();
    }
    
    public bool NoClip 
    { 
        get => stateMachine.CurrentState is NoClipping;
        set => stateMachine.ChangeState(value ? typeof(NoClipping) : typeof(Falling));
    }
    
    #region Helper Properties

    private int WallDirection 
        => rightWall.Touching ? 1
        : leftWall.Touching ? -1
        : 0;

    private bool JumpInput => jumpBuffer.IsBuffered && !IgnoringInput;

    private bool IgnoringInput => Time.time < respawnTime + respawnIgnoreInputDuration; 
    
    private Vector2 MoveInput => IgnoringInput 
        ? Vector2.zero
        : moveInput.action.ReadValue<Vector2>();

    private Vector2 Velocity
    {
        get => rigidbody.linearVelocity;
        set => rigidbody.linearVelocity = value;
    }
    
    #endregion

    private void Awake()
    {
        spawnPoint = transform.position;
        
        bounceable.Bounced += OnBounced;
        fallDamageSource.DamageDealt += OnFallDamageDealt;
        levelResettable.Reset += OnReset;

        InitStateMachine();

        respawnTime = Time.time;

        jumpBuffer = new(jumpBufferDuration);
    }
    
    private void Update()
    {
        if (immobilized) return;
        
        jumpBuffer.BufferUpdate(jumpInput.action.WasPerformedThisFrame());
        
        stateMachine.Update(Time.deltaTime);
    }
    
    #region Event Responses
    
    private void OnReset()
    {
        positionRecorder.AddPosition();
        
        transform.position = checkpointEnjoyer.HasCheckpoint 
            ? checkpointEnjoyer.Checkpoint.transform.position
            : spawnPoint;
        
        Velocity = Vector2.zero;
     
        positionRecorder.NewSegment();

        respawnTime = Time.time;
    }

    private void OnBounced(BounceParams bounceParams)
    {
        stateMachine.ChangeState<Bouncing>();

        rigidbody.MovePosition(bounceParams.SnapPosition(rigidbody.position));
        Velocity = bounceParams.force;
    }

    private void OnFallDamageDealt()
    {
        stateMachine.ChangeState<Bouncing>();
    }

    #endregion
    
    #region Movement States

    private void InitStateMachine()
    {
        stateMachine = new();
        
        TransitionCondition

            canGround = () => ground.IsHitting,

            canJump = () => JumpInput && ground.IsHitting,
            canCoyoteJump = () => JumpInput && stateMachine.PreviousState is Grounded && stateMachine.StateDuration < coyoteTime,
            canDoubleJump = () => JumpInput && extraJumpsUsed < maxExtraJumps,
            
            canEndJump = () => !jumpInput.action.IsPressed(),
            canEndBounce = () => !jumpInput.action.IsPressed() && stateMachine.StateDuration > minBounceDuration,
            
            canEndJumpPeak = () => Velocity.y <= 0,

            canWalljump = () => walljumpingEnabled && WallDirection != 0 && JumpInput,
            
            canWallSlide = () => wallSlidingEnabled && WallDirection != 0 && WallDirection == MoveInput.x && !ground.IsHitting,
            canEndWallSlide = () => WallDirection == 0 || MoveInput.x.Sign0() == -WallDirection,

            canFall = () => !ground.IsHitting;
        
        stateMachine.AddState<Grounded>(new())
            .AddTransition<Jumping>(canJump)
            .AddTransition<Falling>(canFall);
        
        // Jump Types
        stateMachine.AddState<Jumping>(new())
            .AddTransition<Falling>(canEndJumpPeak)
            .AddTransition<Peaking>(canEndJump);
        stateMachine.AddState<DoubleJumping>(new())
            .AddTransition<Falling>(canEndJumpPeak)
            .AddTransition<Peaking>(canEndJump);
        stateMachine.AddState<Bouncing>(new())
            .AddTransition<Falling>(canEndJumpPeak)
            .AddTransition<Peaking>(canEndBounce);
        stateMachine.AddState<WallJumping>(new())
            .AddTransition<Falling>(canEndJumpPeak)
            .AddTransition<Grounded>(canGround)
            .AddTransition<Peaking>(canEndJump);

        stateMachine.AddState<Peaking>(new())
            .AddTransition<Falling>(canEndJumpPeak)
            .AddTransition<WallJumping>(canWalljump);

        stateMachine.AddState<Falling>(new())
            .AddTransition<Jumping>(canCoyoteJump)
            .AddTransition<Grounded>(canGround)
            .AddTransition<WallJumping>(canWalljump)
            .AddTransition<WallSliding>(canWallSlide)
            .AddTransition<DoubleJumping>(canDoubleJump);

        stateMachine.AddState<WallSliding>(new())
            .AddTransition<Falling>(canEndWallSlide)
            .AddTransition<WallJumping>(canWalljump)
            .AddTransition<Grounded>(canGround);

        stateMachine.AddState<NoClipping>(new());
        
        stateMachine.InitializeAllStatesWithContext(this);
        
        stateMachine.SetDefaultState<Grounded>();
    }
    
    private class State : ContextStateBehavior<PlayerMovement>
    {
        protected float VelocityX
        {
            get => Velocity.x;
            set => Velocity = new(value, Velocity.y);
        }
        
        protected float VelocityY
        {
            get => Velocity.y;
            set => Velocity = new(Velocity.x, value);
        }

        protected Vector2 Velocity
        {
            get => Context.Velocity;
            set => Context.Velocity = value;
        }
        
        protected void AirRun()
        {
            float activeAirControlMultiplier = Mathf.Lerp(1, Context.peakAirControlMultiplier,
                Mathf.InverseLerp(Context.peakAirControlThreshold, 0f, Mathf.Abs(VelocityY)));
            
            Run(Context.airAccel * activeAirControlMultiplier, Context.airDeccel);
        }

        protected void Run(float accel, float deccel) =>
            Run(accel, deccel, Context.MoveInput.x);

        protected void Run(float accel, float deccel, float input) =>
            VelocityX = Mathf.MoveTowards(VelocityX, input * Context.runSpeed,
                (input != 0 ? accel : deccel) * Time.deltaTime);

        protected void Fall(float gravity) => 
            VelocityY = Mathf.MoveTowards(VelocityY, -Context.maxFallSpeed, gravity * Time.deltaTime);
    }

    private class NoClipping : State
    {
        public override void Enter()
        {
            base.Enter();

            Context.collider.enabled = false;
        }

        public override void Update()
        {
            Vector2 targetVelocity = Context.MoveInput * Context.noClipSpeed;
            float accel = Context.MoveInput != Vector2.zero ? Context.noClipAcceleration : Context.noClipDecceleration;
            Velocity = Vector2.MoveTowards(Velocity, targetVelocity, accel * Time.deltaTime);
            
            base.Update();
        }

        public override void Exit()
        {
            Context.collider.enabled = true;
            
            base.Exit();
        }
    }
    
    private class Grounded : State
    {
        public override void Enter()
        {
            base.Enter();
            
            Context.extraJumpsUsed = 0;

            Context.heavyObject.grounded = true;
        }

        public override void Update()
        {
            Run(Context.groundAccel, Context.groundDeccel);

            VelocityY = -Context.groundDownForce;
            
            base.Update();
        }

        public override void Exit()
        {
            Context.heavyObject.grounded = false;
            
            base.Exit();
        }
    }
    
    private class Jumping : State
    {
        public override void Enter()
        {
            base.Enter();

            float velocity = Mathf.Sqrt(2f * Context.jumpHeight * Context.jumpGravity);
            VelocityY = velocity;
            
            Context.jumpBuffer.Reset();
            
            Context.jumpSound.Play();

            Context.jumpDamageSource.enabled = true;

            Context.groundSensor.enabled = true;
        }

        public override void Update()
        {
            AirRun();
            Fall(Context.jumpGravity);

            base.Update();
        }
    }

    private class Bouncing : Jumping
    {
        public override void Enter()
        {
            base.Enter();
            
            Context.extraJumpsUsed = 0;
        }
    }

    private class DoubleJumping : Jumping
    {
        public override void Enter()
        {
            base.Enter();
            
            Context.extraJumpsUsed++;
            
            Context.jumpBuffer.Reset();
        }
    }

    private class Peaking : State
    {
        public override void Update()
        {
            AirRun();
            Fall(Context.peakingGravity);
            
            base.Update();
        }

        public override void Exit()
        {
            Context.jumpDamageSource.enabled = false;

            Context.groundSensor.enabled = false;

            base.Exit();
        }
    }
    
    private class Falling : State
    {
        public override void Enter()
        {
            base.Enter();

            Context.fallDamageSource.enabled = true;
        }

        public override void Update()
        {
            AirRun();
            Fall(Context.fallGravity);
            
            base.Update();
        }

        public override void Exit()
        {
            Context.fallDamageSource.enabled = false;

            base.Exit();
        }
    }
    
    private class WallJumping : State
    {
        public override void Enter()
        {
            base.Enter();
            
            Context.extraJumpsUsed = 0;

            Velocity = new Vector2
            {
                y = Mathf.Sqrt(2f * Context.walljumpHeight * Context.jumpGravity),
                x = Context.walljumpBoost * -Context.WallDirection,
            };
            
            Context.jumpBuffer.Reset();
            
            Context.jumpSound.Play();
        }

        public override void Update()
        {
            float input = Context.MoveInput.x;
            
            if (input.Sign0() == Context.WallDirection)
            {
                input = 0;
            }
            
            Run(Context.airAccel, Context.airDeccel, input);

            float gravity = Context.jumpInput.action.IsPressed()
                ? Context.jumpGravity
                : Context.peakingGravity;
            
            Fall(gravity);

            base.Update();
        }
    }
    
    private class WallSliding : State
    {
        public override void Enter()
        {
            base.Enter();
            
            VelocityY = Mathf.Max(VelocityY, -Context.startWallSlideSpeed);
        }

        public override void Update()
        {
            VelocityY = Mathf.MoveTowards(VelocityY,
                -Context.maxWallSlideSpeed, Context.wallSlideAccel * Time.deltaTime);

            base.Update();
        }
    }
    
    #endregion
}