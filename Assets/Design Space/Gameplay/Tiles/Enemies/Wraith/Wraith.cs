using System;
using UnityEngine;
using UnityEngine.Events;

public class Wraith : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveAcceleration;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private TargetingStrategy detectionTargeting;
    [SerializeField] private TargetingStrategy followTargeting;
    [SerializeField] private Transform visuals;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite passiveSprite;
    [SerializeField] private Sprite scaredSprite;
    [SerializeField] private Sprite followingSprite;
    [SerializeField] private UnityEvent lookingToward;
    [SerializeField] private UnityEvent lookingAway;
    
    private bool wasLookedAt;
    private Targetable target;
    
    private void Update()
    {
        Vector2 targetVelocity;

        if (target == null && detectionTargeting.activeTarget != null)
        {
            target = detectionTargeting.activeTarget;
            followTargeting.activeTarget = target;
        }

        if (followTargeting.activeTarget != target)
        {
            target = null;
        }
        
        if (target != null)
        {
            Vector2 toTarget = target.transform.position - transform.position;

            bool lookedAt = Vector2.Dot(toTarget, target.facingDirection) < 0f;
            
            targetVelocity = lookedAt
                ? Vector2.zero
                : toTarget.normalized * moveSpeed;
            
            spriteRenderer.sprite = lookedAt
                ? scaredSprite
                : followingSprite;

            if (lookedAt && !wasLookedAt)
            {
                lookingToward.Invoke();
            }

            if (!lookedAt && wasLookedAt)
            {
                lookingAway.Invoke();
            }

            wasLookedAt = lookedAt;
        }
        else
        {
            spriteRenderer.sprite = passiveSprite;
            targetVelocity = Vector2.zero;
        }

        rigidbody.linearVelocity = Vector2.MoveTowards(
            rigidbody.linearVelocity,
            targetVelocity,
            moveAcceleration * Time.deltaTime);

        if (rigidbody.linearVelocity != Vector2.zero)
        {
            visuals.localScale = new Vector3(Mathf.Sign(rigidbody.linearVelocity.x), 1f, 1f);
        }

    }
}
