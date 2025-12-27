using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class BounceSource : MonoBehaviour
{
    [SerializeField] private float force;
    public Vector2 direction;
    [SerializeField] private BounceSnapAxis snapAxis;
    [SerializeField] private Transform bouncePosition;
    [SerializeField] private UnityEvent bounceSound;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Bounceable bounceable))
        {
            bounceable.Bounce(new(bouncePosition.position, snapAxis, direction.normalized * force));
            bounceSound.Invoke();
        }
    }
}
