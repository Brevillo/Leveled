using System;
using UnityEngine;

public class TileEntity : MonoBehaviour
{
    public int LayerID { get; private set; }
    public Vector2Int CellPosition { get; private set; }

    public event Action Initialized;
    
    public void Initialize(int layerID, Vector2Int cellPosition)
    {
        LayerID = layerID;
        CellPosition = cellPosition;
        
        Initialized?.Invoke();
    }
}
