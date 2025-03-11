using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = CreateMenuPath + "Selection", order = CreateMenuOrder)]
public class SelectionToolAction : ToolbarAction
{
    [SerializeField] private InputActionReference copyInput;
    [SerializeField] private InputActionReference pasteInput;

    private bool selectionCopied;
    private State state;

    private Vector2Int clipboardAnchor;
    private List<Vector2Int> clipboardPositions;
    private List<TileData> clipboardTiles;
    
    private enum State
    {
        None,
        Selecting,
        Selected,
        MovingSelection,
    }

    protected override void OnActivated()
    {
        copyInput.action.performed += Copy;
        pasteInput.action.performed += Paste;
    }

    protected override void OnDeactivated()
    {
        copyInput.action.performed -= Copy;
        pasteInput.action.performed -= Paste;
    }

    protected override void OnDown()
    {
        if (activeToolSide == ToolbarActionsManager.ToolSide.Secondary)
        {
            state = State.None;
            return;
        }
        
        selectionCopied = false;

        switch (state)
        {
            case State.None:

                state = State.Selecting;
                        
                break;
                    
            case State.Selected:

                if (blackboard.selection.Contains((Vector3Int)dragStart))
                {
                    state = State.MovingSelection;
                }
                        
                break;
        }
        
        if (state != State.MovingSelection || !blackboard.selection.Contains((Vector3Int)SpaceUtility.MouseCell))
        {
            state = State.Selecting;
        }
    }

    protected override void OnUpdate()
    {
        switch (state)
        {
            case State.Selecting:
                
                blackboard.hoverSelectionCenter = SelectionCenter;
                blackboard.hoverSelectionSize = SelectionSize;
                
                break;
            
            case State.MovingSelection:
                
                blackboard.hoverSelectionCenter = blackboard.selection.center + SpaceUtility.MouseCellCenterWorld -
                                                  blackboard.spaceUtility.CellToWorld(dragStart);
                blackboard.hoverSelectionSize = (Vector3)blackboard.selection.size;
                
                break;
            
            case State.Selected:

                if (!selectionCopied) break;
                
                Vector3 selectionCenterOffset = SpaceUtility.GetBoundsIntCenterWorld(blackboard.selection);
                blackboard.hoverSelectionCenter = blackboard.spaceUtility.SnapWorldToCell(
                    (Vector2)(SpaceUtility.MouseWorld - selectionCenterOffset)
                    + Vector2.one / 2f) + selectionCenterOffset;
                
                blackboard.hoverSelectionSize = (Vector3)blackboard.selection.size;
                
                break;
        }

        blackboard.selectionOutlineActive = state is State.Selected or State.MovingSelection;
    }

    protected override void OnReleased()
    {
        switch (state)
        {
            case State.Selecting:

                blackboard.selection = GetCurrentSelection();

                state = State.Selected;
                        
                break;
                        
            case State.MovingSelection:

                state = State.Selected;

                if (dragStart == SpaceUtility.MouseCell) break;

                List<Vector2Int> originPositions = new();
                foreach (Vector2Int position in blackboard.selection.allPositionsWithin)
                {
                    originPositions.Add(position);
                }

                var nullTiles = new TileData[originPositions.Count];

                var originTiles = originPositions
                    .Select(EditorState.GetTile)
                    .ToArray();

                Vector2Int delta = SpaceUtility.MouseCell - dragStart;
                var destinationPositions = originPositions
                    .Select(position => position + delta)
                    .ToArray();

                EditorState.StartChangeBundle("Moved tile selection");

                EditorState.SetTiles(originPositions.ToArray(), nullTiles, "Deleted original selection");
                EditorState.SetTiles(destinationPositions, originTiles, "Filled new selection");

                EditorState.EndChangeBundle();

                blackboard.selection.position += (Vector3Int)delta;

                break;
        }
    }
    
    private void Copy(InputAction.CallbackContext context)
    {
        if (state != State.Selected) return;

        selectionCopied = true;
        clipboardAnchor = (Vector2Int)blackboard.selection.position;

        clipboardPositions ??= new();
        clipboardTiles ??= new();
        
        clipboardPositions.Clear();
        clipboardTiles.Clear();

        foreach (Vector2Int position in blackboard.selection.allPositionsWithin)
        {
            clipboardPositions.Add(position);
            clipboardTiles.Add(blackboard.editorState.GetTile(position));
        }
    }
    
    private void Paste(InputAction.CallbackContext context)
    {
        if (!selectionCopied) return;
        
        Vector3 selectionCenterOffset = blackboard.spaceUtility.GetBoundsIntCenterWorld(blackboard.selection);
        Vector2Int selectionCenter = blackboard.spaceUtility.WorldToCell(
            blackboard.spaceUtility.SnapWorldToCell((Vector2)(blackboard.spaceUtility.MouseWorld - selectionCenterOffset) + Vector2.one / 2f) +
            selectionCenterOffset - (Vector3)blackboard.selection.size / 2f);
        
        Vector2Int delta = selectionCenter - clipboardAnchor;

        blackboard.editorState.SetTiles(
            clipboardPositions.Select(position => position + delta).ToArray(),
            clipboardTiles.ToArray(),
            "Pasted tiles");
    }
}
