using System;
using UnityEngine;
using UnityEngine.Events;

public class Bounceable : MonoBehaviour
{
    [SerializeField] private UnityEvent<Vector2> bounced;
    public event Action<Vector2> Bounced; 

    public void Bounce(Vector2 force)
    {
        Bounced?.Invoke(force);
        bounced.Invoke(force);
    }
}
