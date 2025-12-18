using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class CollisionEvents : MonoBehaviour
{
    [SerializeField] private UnityEvent<Collision> collisionEnter, collisionStay, collisionExit;
    [SerializeField] private UnityEvent<Collider> triggerEnter, triggerStay, triggerExit;

    public event Action<Collision> CollisionEnter, CollisionStay, CollisionExit;
    public event Action<Collider> TriggerEnter, TriggerStay, TriggerExit;

    private void OnCollisionEnter(Collision collision)
    {
        CollisionEnter?.Invoke(collision);
        collisionEnter.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CollisionStay?.Invoke(collision);
        collisionStay.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        CollisionExit?.Invoke(collision);
        collisionExit.Invoke(collision);
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnter?.Invoke(other);
        triggerEnter.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TriggerStay?.Invoke(other);
        triggerStay.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        TriggerExit?.Invoke(other);
        triggerExit.Invoke(other);
    }
}
