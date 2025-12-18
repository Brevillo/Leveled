using UnityEngine;

public class Teleportable : MonoBehaviour
{
    [SerializeField] private new Rigidbody2D rigidbody;

    public void Teleport(Vector3 newPosition)
    {
        rigidbody.position = newPosition;
    }
}
