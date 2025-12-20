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
    
    [Header("References")] 
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private Transform visuals;
    [SerializeField] private PositionRecorder positionRecorder;
    [SerializeField] private Bounceable bounceable;
    [SerializeField] private Targetable targetable;
    [SerializeField] private HeavyObject heavyObject;

    private Vector2 spawnPoint;
    private Checkpoint checkpoint;

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

    public void Immobilize(bool immobilized)
    {
        this.immobilized = immobilized;
        
        stateMachine.Reset();
    }
    
    private Vector2 MoveInput => IgnoringInput 
        ? Vector2.zero
        : moveInput.action.ReadValue<Vector2>();

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

        respawnTime = Time.time;
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
        jumpDamageSource.DamageDealt += OnJumpDamageDealt;

        respawnTime = Time.time;

        jumpBuffer = new(jumpBufferDuration);
    }

    private void OnBounced(BounceParams bounceParams)
    {
        stateMachine.ChangeState<Bouncing>();

        rigidbody.MovePosition(bounceParams.SnapPosition(rigidbody.position));
        rigidbody.linearVelocity = bounceParams.force;
    }

    private void OnJumpDamageDealt()
    {
        stateMachine.ChangeState<Bouncing>();
    }

    private void Update()
    {
        if (immobilized) return;
        
        jumpBuffer.BufferUpdate(jumpInput.action.WasPerformedThisFrame());
        
        stateMachine.Update(Time.deltaTime);

        if (MoveInput.x != 0f)
        {
            targetable.facingDirection = new(MoveInput.x, 0f);
            visuals.localScale = new(Mathf.Sign(MoveInput.x), 1f, 1f);
        }
    }

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
            canEndWallSlide = () => WallDirection == 0 || Sign0(MoveInput.x) == -WallDirection,

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

        stateMachine.InitializeAllStatesWithContext(this);
        
        stateMachine.SetDefaultState<Grounded>();
    }

    private class State : IContextStateBehavior<PlayerMovement>
    {
        public float StateDuration { get; set; }
        public PlayerMovement Context { get; set; }

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

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
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
            Context.extraJumpsUsed = 0;
            
            base.Enter();
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
    }
    
    private class Falling : State
    {
        public override void Enter()
        {
            base.Enter();

            Context.jumpDamageSource.enabled = true;
        }

        public override void Update()
        {
            AirRun();
            Fall(Context.fallGravity);
            
            base.Update();
        }

        public override void Exit()
        {
            Context.jumpDamageSource.enabled = false;

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
            
            if (Sign0(input) == Context.WallDirection)
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
    
    /*
    private class Template : State
    {
        public Template(PlayerMovement context) : base(context) { }
    }
    */
}
