using UnityEngine;

public class Bounceable : MonoBehaviour
{
    [SerializeField] private new Rigidbody2D rigidbody;

    public void Bounce(Vector2 force)
    {
        rigidbody.linearVelocity = force;
    }
}
