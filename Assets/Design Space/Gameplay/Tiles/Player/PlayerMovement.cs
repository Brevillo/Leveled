using OliverBeebe.UnityUtilities.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float respawnIgnoreInputDuration;
    
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
    [SerializeField] private float peakAirControlMultiplier;
    [SerializeField] private float peakAirControlThreshold;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpBufferDuration;
    [SerializeField] private CollisionAggregate2D ground;
    [SerializeField] private SoundEffect jumpSound;
    [SerializeField] private DamageSource jumpDamageSource;
    [SerializeField] private DamageSource fallDamageSource;

    [Header("Bouncing")]
    [SerializeField] private float minBounceDuration;
    
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

    private Vector2 spawnPoint;

    private float respawnTime;
    private int extraJumpsUsed;

    private BufferTimer jumpBuffer;
    
    private StateMachine stateMachine;

    private bool immobilized;

    private int WallDirection 
        => rightWall.Touching ? 1
        : leftWall.Touching ? -1
        : 0;

    private bool JumpInput => jumpBuffer.IsBuffered && !IgnoringInput;

    private bool IgnoringInput => Time.time < respawnTime + respawnIgnoreInputDuration; 
    
    private Vector2 MoveInput => IgnoringInput 
        ? Vector2.zero
        : moveInput.action.ReadValue<Vector2>();

    public bool NoClip 
    { 
        get => stateMachine.CurrentState is NoClipping;
        set => stateMachine.ChangeState(value ? typeof(NoClipping) : typeof(Falling));
    }

    public void Immobilize(bool immobilized)
    {
        this.immobilized = immobilized;
        
        stateMachine.Reset();
    }
    
    private void Respawn()
    {
        positionRecorder.AddPosition();

        rigidbody.interpolation = RigidbodyInterpolation2D.None;
        transform.position = checkpointEnjoyer.HasCheckpoint 
            ? checkpointEnjoyer.Checkpoint.transform.position
            : spawnPoint;
        rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        rigidbody.linearVelocity = Vector2.zero;
     
        positionRecorder.NewSegment();

        respawnTime = Time.time;
    }

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

    #region Event Responses
    
    private void OnReset()
    {
        Respawn();
    }

    private void OnBounced(BounceParams bounceParams)
    {
        stateMachine.ChangeState<Bouncing>();

        rigidbody.MovePosition(bounceParams.SnapPosition(rigidbody.position));
        rigidbody.linearVelocity = bounceParams.force;
    }

    private void OnFallDamageDealt()
    {
        stateMachine.ChangeState<Bouncing>();
    }

    #endregion
    
    private void Update()
    {
        if (immobilized) return;
        
        jumpBuffer.BufferUpdate(jumpInput.action.WasPerformedThisFrame());
        
        stateMachine.Update(Time.deltaTime);
    }

    #region Movement States

    private void InitStateMachine()
    {
        stateMachine = new();
        
        TransitionCondition

            canGround = () => ground.Touching,

            canJump = () => JumpInput && ground.Touching,
            canCoyoteJump = () => JumpInput && stateMachine.PreviousState is Grounded && stateMachine.StateDuration < coyoteTime,
            canDoubleJump = () => JumpInput && extraJumpsUsed < maxExtraJumps,
            
            canEndJump = () => !jumpInput.action.IsPressed(),
            canEndBounce = () => !jumpInput.action.IsPressed() && stateMachine.StateDuration > minBounceDuration,
            
            canEndJumpPeak = () => rigidbody.linearVelocityY <= 0,

            canWalljump = () => WallDirection != 0 && JumpInput,
            canEndWalljump = () => stateMachine.StateDuration > walljumpNoTurnDuration
                                  || rigidbody.linearVelocityY <= 0,

            canWallSlide = () => WallDirection != 0 && WallDirection == MoveInput.x && !ground.Touching,
            canEndWallSlide = () => WallDirection == 0 || MoveInput.x.Sign0() == -WallDirection,

            canFall = () => !ground.Touching;
        
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
            get => Context.rigidbody.linearVelocityX;
            set => Context.rigidbody.linearVelocityX = value;
        }
        
        protected float VelocityY
        {
            get => Context.rigidbody.linearVelocityY;
            set => Context.rigidbody.linearVelocityY = value;
        }

        protected Vector2 Velocity
        {
            get => Context.rigidbody.linearVelocity;
            set => Context.rigidbody.linearVelocity = value;
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
            
            Context.rigidbody.linearVelocityY = Mathf.Sqrt(2f * Context.jumpHeight * Context.jumpGravity);
            
            Context.jumpBuffer.Reset();
            
            Context.jumpSound.Play();

            Context.jumpDamageSource.enabled = true;
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

            Context.rigidbody.linearVelocity = new Vector2
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
            
            Context.rigidbody.linearVelocityY = Mathf.Max(Context.rigidbody.linearVelocityY, -Context.startWallSlideSpeed);
        }

        public override void Update()
        {
            Context.rigidbody.linearVelocityY = Mathf.MoveTowards(Context.rigidbody.linearVelocityY,
                -Context.maxWallSlideSpeed, Context.wallSlideAccel * Time.deltaTime);

            base.Update();
        }
    }
    
    #endregion
}