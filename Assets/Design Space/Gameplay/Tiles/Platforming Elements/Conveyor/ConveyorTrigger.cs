using System;
using UnityEngine;

public class ConveyorTrigger : MonoBehaviour
{
    [SerializeField] private Vector2 speed;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out Rigidbody2D rigidbody))
        {
            rigidbody.position += speed * Time.fixedDeltaTime;
        }
    }
}
