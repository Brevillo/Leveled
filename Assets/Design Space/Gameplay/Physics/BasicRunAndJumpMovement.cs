using System;
using UnityEngine;

public class BasicRunAndJumpMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpFrequency;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private CollisionAggregate2D rightBouncer;
    [SerializeField] private CollisionAggregate2D leftBouncer;
    [SerializeField] private CollisionAggregate2D downBouncer;

    private float groundedTimer;
    
    private float JumpVelocity => Mathf.Sqrt(2f * gravity * jumpHeight);

    private void Start()
    {
        rigidbody.linearVelocity = new Vector2(moveSpeed, JumpVelocity);
    }

    private void Update()
    {
        Vector2 velocity = rigidbody.linearVelocity;

        if (rightBouncer.Touching)
        {
            velocity.x = -moveSpeed;
        }

        if (leftBouncer.Touching)
        {
            velocity.x = moveSpeed;
        }

        if (downBouncer.Touching)
        {
            groundedTimer += Time.deltaTime;
            
            if (groundedTimer > jumpFrequency)
            {
                velocity.y = JumpVelocity;
            }
        }
        else
        {
            groundedTimer = 0f;
        }


        velocity.y -= gravity * Time.deltaTime;
        
        rigidbody.linearVelocity = velocity;
    }
}
