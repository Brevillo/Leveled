using System;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] private float force;
    [SerializeField] private Vector2 direction;
    [SerializeField] private BounceSnapAxis snapAxis;
    [SerializeField] private Transform bouncePosition;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Bounceable bounceable))
        {
            bounceable.Bounce(new(bouncePosition.position, snapAxis, direction.normalized * force));
        }
    }
}
