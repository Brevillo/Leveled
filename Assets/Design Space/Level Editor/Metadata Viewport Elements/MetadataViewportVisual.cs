using System;
using UnityEngine;

public class MetadataViewportVisual : MonoBehaviour
{
    public event Action<object> VisualUpdated;
    
    public void UpdateVisual(object metadata)
    {
        VisualUpdated?.Invoke(metadata);
    }
}