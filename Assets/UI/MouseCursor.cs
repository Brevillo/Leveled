using System;
using UnityEditor;
using UnityEngine;
using static ToolbarActionsManager;

public class MouseCursor : MonoBehaviour
{
    [SerializeField] private Transform gridSpace;
    [SerializeField] private Grid grid;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ToolbarActionsManager toolbarActions;
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
        
        (Vector3 position, Vector3 size) = toolbarActions.CurrentState switch
        {
            State.DrawingRect or State.Selecting => (
                position: (mouseWorld + grid.GetCellCenterWorld(toolbarActions.DragStart)) / 2f,
                size: new Vector3(
                    Mathf.Abs(mouseCell.x - toolbarActions.DragStart.x) + 1,
                    Mathf.Abs(mouseCell.y - toolbarActions.DragStart.y) + 1,
                    1)),

            State.MovingSelection => (
                position: toolbarActions.Selection.center + mouseWorld - grid.GetCellCenterWorld(toolbarActions.DragStart),
                size: new Vector3(
                    toolbarActions.Selection.size.x * grid.cellSize.x,
                    toolbarActions.Selection.size.y * grid.cellSize.y,
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
