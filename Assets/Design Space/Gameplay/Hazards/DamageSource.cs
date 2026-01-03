using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DamageSource : MonoBehaviour
{
    [SerializeField] private List<DamageType> damageTypes;
    [SerializeField] private bool noDamageToFallingPlayer;
    [SerializeField] private List<Range> validDamageRanges;
    [SerializeField] private UnityEvent damageDealt;
    
    [Serializable]
    private class Range
    {
        public Vector2 direction;
        public float arcWidth;

        public bool Within(float angle) =>
            Mathf.Abs(Mathf.DeltaAngle(angle, Vector2.SignedAngle(Vector2.right, direction))) < arcWidth / 2f;
    }
    
    public event Action DamageDealt;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckForDamage(other);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        CheckForDamage(other.collider);
    }

    private void CheckForDamage(Collider2D other)
    {
        float angle = Vector2.SignedAngle(Vector2.right, other.transform.position - transform.position);
        
        if (!enabled) return;

        if (!other.TryGetComponent(out Damageable damageable)) return;
            
        if (validDamageRanges.Any(range => !range.Within(angle))) return;

        if (noDamageToFallingPlayer && other.bounds.min.y > transform.position.y) return;
        
        var result = damageable.TakeDamage(new(damageTypes));

        if (result.received)
        {
            DamageDealt?.Invoke();
            damageDealt.Invoke();
        }
    }

    private void OnDrawGizmosSelected()
    {
        int linePoints = 20;
        float arcRadius = 2f;

        Gizmos.color = Color.green;

        if (validDamageRanges != null)
        {
            foreach (var range in validDamageRanges)
            {
                float start = Vector2.SignedAngle(Vector2.right, range.direction) - range.arcWidth / 2f;
                
                Gizmos.DrawLineStrip(Enumerable.Range(0, linePoints)
                    .Select(i =>
                    {
                        float angle = (start + (float)i / (linePoints - 1) * range.arcWidth) * Mathf.Deg2Rad;

                        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * arcRadius;
                    })
                    .ToArray(), false);
            }
        }
    }
}
