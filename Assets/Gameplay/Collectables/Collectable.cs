using System;
using UnityEngine;
using UnityEngine.Events;

public class Collectable : MonoBehaviour
{
    [SerializeField] private CollectableRegistry registry;
    
    [SerializeField] private UnityEvent collected;
    public event Action Collected; 
    
    private bool isCollected;

    public bool IsCollected => isCollected;

    private void OnEnable()
    {
        registry.Add(this);
    }

    private void OnDisable()
    {
        registry.Remove(this);
    }

    public void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        Collected?.Invoke();
        collected.Invoke();
    }
}
