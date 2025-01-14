using System;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TilePlacer;

public class MouseCursor : MonoBehaviour
{
    [SerializeField] private Transform gridSpace;
    [SerializeField] private Grid grid;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TilePlacer tilePlacer;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private float moveSpeed;
    [SerializeField] private CursorType[] cursorTypes;
    
    [Serializable]
    private struct CursorType
    {
        public ToolType tool;
        public Sprite cursorSprite;
    }

    private Vector3 positionVelocity;
    private Vector3 sizeVelocity;
    
    private void LateUpdate()
    {
        Vector3Int mouseCell = grid.WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        Vector3 mouseWorld = grid.GetCellCenterWorld(mouseCell);
        
        (Vector3 position, Vector3 size) = tilePlacer.State switch
        {
            PlacerState.DrawingRect or PlacerState.Selecting => (
                position: (mouseWorld + grid.GetCellCenterWorld(tilePlacer.DragStart)) / 2f,
                size: new Vector3(
                    Mathf.Abs(mouseCell.x - tilePlacer.DragStart.x) + 1,
                    Mathf.Abs(mouseCell.y - tilePlacer.DragStart.y) + 1,
                    1)),

            _ => (
                position: mouseWorld,
                size: Vector3.one),
        };

        gridSpace.localScale = Vector3.SmoothDamp(gridSpace.localScale, size, ref sizeVelocity, moveSpeed);
        gridSpace.localPosition =
            Vector3.SmoothDamp(gridSpace.localPosition, position, ref positionVelocity, moveSpeed);
    }
}
