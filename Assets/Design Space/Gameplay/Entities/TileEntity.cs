using System;
using UnityEngine;

public class TileEntity : MonoBehaviour
{
    public int LayerID { get; private set; }
    public Vector2Int CellPosition { get; private set; }
    public Metadata Metadata { get; private set; }
    
    public event Action Initialized;
    
    public void Initialize(int layerID, Vector2Int cellPosition, Metadata metadata)
    {
        LayerID = layerID;
        CellPosition = cellPosition;
        Metadata = metadata;
        
        Initialized?.Invoke();
    }
}
