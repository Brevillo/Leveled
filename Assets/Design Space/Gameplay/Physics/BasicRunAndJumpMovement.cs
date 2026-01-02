using System;
using UnityEngine;

public class BasicRunAndJumpMovement : MonoBehaviour
{
    [SerializeField] private int startDirection = -1;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveAcceleration;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpFrequency;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private Vector2 size;
    [SerializeField] private Vector2 sideCheckSize;
    [SerializeField] private Vector2 downCheckSize;
    [SerializeField] private float groundCheckNormalRange;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Bounceable bounceable;

    private BoxCast2D rightCaster, leftCaster, downCaster;
    
    public bool running = true;

    private int direction;
    private float groundedTimer;
    
    private float JumpVelocity => Mathf.Sqrt(2f * gravity * jumpHeight);
    
    private void Awake()
    {
        if (bounceable != null)
        {
            bounceable.Bounced += OnBounced;
        }
        
        CreateCasters();

        direction = startDirection;
    }

    private void CreateCasters()
    {
        leftCaster = Direction(Vector2.left, sideCheckSize);
        rightCaster = Direction(Vector2.right, sideCheckSize);
        downCaster = Direction(Vector2.down, downCheckSize);

        BoxCast2D Direction(Vector2 direction, Vector2 size) =>
            new(Vector2.zero,
                size,
                0f,
                direction,
                new ContactFilter2D()
                    .LayerMask(groundMask)
                    .Normal(-direction, groundCheckNormalRange),
                originTransform: transform); 
    }

    private void OnBounced(BounceParams bounceParams)
    {
        rigidbody.MovePosition(bounceParams.SnapPosition(rigidbody.position));
        rigidbody.linearVelocity = bounceParams.force;
    }

    private void Start()
    {
        rigidbody.linearVelocity = new Vector2(moveSpeed, JumpVelocity);
    }

    private void FixedUpdate()
    {
        rightCaster.Update();
        leftCaster.Update();
        downCaster.Update();
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.IsPlaying(this))
        {
            CreateCasters();
        }
        
        rightCaster.DrawGizmos();
        leftCaster.DrawGizmos();
        downCaster.DrawGizmos();
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, size);
    }

    private void Update()
    {
        if (rightCaster.WithinDistance(transform, size.x / 2f))
        {
            direction = -1;
        }

        if (leftCaster.WithinDistance(transform, size.x / 2f))
        {
            direction = 1;
        }

        float targetSpeed = running ? direction * moveSpeed : 0f;
        rigidbody.linearVelocityX =
            Mathf.MoveTowards(rigidbody.linearVelocityX, targetSpeed, moveAcceleration * Time.deltaTime); 

        if (downCaster.WithinDistance(transform, size.y / 2f) && rigidbody.linearVelocityY <= 0f)
        {
            groundedTimer += Time.deltaTime;

            rigidbody.linearVelocityY = 0f;
            
            if (groundedTimer > jumpFrequency)
            {
                rigidbody.linearVelocityY = JumpVelocity;
            }
        }
        else
        {
            groundedTimer = 0f;
            rigidbody.linearVelocity += Vector2.down * (gravity * Time.deltaTime);
        }
    }
}
