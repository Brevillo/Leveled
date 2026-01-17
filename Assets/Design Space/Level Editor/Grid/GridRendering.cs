using System;
using UnityEngine;

public class GridRendering : MonoBehaviour
{
    [SerializeField] private Material gridMaterial;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private float thickness;

    private static readonly int cellSizeID = Shader.PropertyToID("_Cell_Size");
    private static readonly int originID = Shader.PropertyToID("_Origin");
    private static readonly int thicknessID = Shader.PropertyToID("_Thickness");
    
    private void LateUpdate()
    {
        Vector2 origin = 
            spaceUtility.WorldToScreen((Vector2)spaceUtility.CellToWorld(Vector2Int.zero) + Vector2.one * 0.5f);
        gridMaterial.SetVector(originID, origin);
        
        Vector2 cellSize = spaceUtility.CellToScreen(Vector2Int.one) - spaceUtility.CellToScreen(Vector2Int.zero);
        gridMaterial.SetVector(cellSizeID, cellSize);
        
        gridMaterial.SetFloat(thicknessID, thickness);
    }
}
