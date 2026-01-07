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

    private HashSet<VelocityResolver> activeResolvers;
    private Coroutine fallCoroutine;

    private void Awake()
    {
        activeResolvers = new();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.TryGetComponent(out VelocityResolver velocityResolver))
        {
            activeResolvers.Add(velocityResolver);

            if (fallCoroutine == null && other.collider.TryGetComponent(out PlayerMovement _) &&
                velocityResolver.Rigidbody.linearVelocityY <= 0f)
            {
                fallCoroutine = StartCoroutine(Fall());
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.TryGetComponent(out VelocityResolver velocityResolver))
        {
            velocityResolver.UnsetVelocity(this);
            activeResolvers.Remove(velocityResolver);
        }
    }

    private IEnumerator Fall()
    {
        yield return new WaitForSeconds(fallDelay);

        rigidbody.linearVelocityY = -startFallSpeed;
        
        while (transform.position.y > tilePlacerReference.value.Rect.min.y - destroyOffScreenBuffer)
        {
            rigidbody.linearVelocityY = Mathf.MoveTowards(
                rigidbody.linearVelocityY,
                -maxFallSpeed,
                fallGravity * Time.deltaTime);

            foreach (var resolver in activeResolvers)
            {
                resolver.SetVelocity(this, rigidbody.linearVelocity);
            }

            yield return null;
        }
    }
}
