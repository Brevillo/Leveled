using System;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseCursor : MonoBehaviour
{
    [SerializeField] private Transform gridSpace;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TilePlacer tilePlacer;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private CursorType[] cursorTypes;
    
    [Serializable]
    private struct CursorType
    {
        public ToolType tool;
        public Sprite cursorSprite;
    }    
    
    private void LateUpdate()
    {
        Vector3Int mouseCell = tilemap.WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        
        if (tilePlacer.DragStart != null)
        {
            var rectStart = (Vector3Int)tilePlacer.DragStart;
            
            gridSpace.localScale = new(
                Mathf.Abs(mouseCell.x - rectStart.x) + 1,
                Mathf.Abs(mouseCell.y - rectStart.y) + 1,
                1);
            gridSpace.position = (tilemap.GetCellCenterWorld(mouseCell) +
                                  tilemap.GetCellCenterWorld(rectStart)) / 2f;
        }
        else
        {
            gridSpace.localScale = Vector3.one;
            gridSpace.position = tilemap.GetCellCenterWorld(mouseCell);
        }
    }
}
