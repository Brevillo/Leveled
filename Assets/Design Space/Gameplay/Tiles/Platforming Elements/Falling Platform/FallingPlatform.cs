using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay;
    [SerializeField] private float startFallSpeed;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float fallGravity;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private float destroyOffScreenBuffer;
    [SerializeField] private TilePlacerReference tilePlacerReference;
    
    private Coroutine fallCoroutine;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.TryGetComponent(out PlayerMovement player))
        {
            if (fallCoroutine == null && other.collider.TryGetComponent(out Rigidbody2D playerRigidbody) &&
                playerRigidbody.linearVelocityY <= 0f)
            {
                fallCoroutine = StartCoroutine(Fall());
            }
        }
    }
    
    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(fallDelay);

        Vector2 velocity = Vector2.down * startFallSpeed;
        
        while (transform.position.y > tilePlacerReference.value.Rect.min.y - destroyOffScreenBuffer)
        {
            velocity.y = Mathf.MoveTowards(
                velocity.y,
                -maxFallSpeed,
                fallGravity * Time.deltaTime);

            transform.position += (Vector3)velocity * Time.deltaTime;
            
            yield return null;
        }
    }
}
