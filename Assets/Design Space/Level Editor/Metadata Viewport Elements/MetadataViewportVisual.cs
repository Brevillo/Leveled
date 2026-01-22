using System;
using UnityEngine;

public class MetadataViewportVisual : MonoBehaviour
{
    public Vector2Int CellPosition { get; private set; }
    public int LayerID { get; private set; }

    public event Action Initialized;
    public event Action<object> MetadataUpdated;
    public event Action PositionUpdated;
    
    public void Initialize(Vector2Int cellPosition, int layerID)
    {
        CellPosition = cellPosition;
        LayerID = layerID;
    }
    
    public void UpdateMetadata(object metadata)
    {
        MetadataUpdated?.Invoke(metadata);
    }

    public void UpdatePosition()
    {
        PositionUpdated?.Invoke();
    }
}