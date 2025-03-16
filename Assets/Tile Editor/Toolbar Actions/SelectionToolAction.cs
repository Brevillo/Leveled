using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = CreateMenuPath + "Selection", order = CreateMenuOrder)]
public class SelectionToolAction : ToolbarAction
{
    [SerializeField] private string pasteChangelogMessage;
    [SerializeField] private string cutChangelogMessage;
    
    private bool selectionCopied;
    private State state;

    private Vector2Int clipboardAnchor;
    private List<Vector2Int> clipboardPositions;
    private List<TileData> clipboardTiles;

    protected Vector2Int CopySelectionMin
    {
        get
        {
            Vector3 selectionCenterOffset = SpaceUtility.GetBoundsIntCenterWorld(blackboard.selection);
            Vector3 worldPosition = SpaceUtility.MouseWorld + (Vector3)Vector2.one / 2f;
            Vector3 snappedWorldCenter = 
                SpaceUtility.SnapWorldToCell(worldPosition - selectionCenterOffset) + selectionCenterOffset;
            
            return SpaceUtility.WorldToCell(snappedWorldCenter - (Vector3)blackboard.selection.size / 2f);
        }
    }
    
    private enum State
    {
        None,
        Selecting,
        Selected,
        MovingSelection,
    }

    protected override void OnDown()
    {
        if (activeToolSide == ToolSide.Secondary)
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

                blackboard.hoverSelection = Selection;
                
                break;
            
            case State.MovingSelection:
                
                blackboard.hoverSelection = new(blackboard.selection.position + (Vector3Int)(SpaceUtility.MouseCell - dragStart), blackboard.selection.size);
                
                break;
            
            case State.Selected:

                if (!selectionCopied) break;
                
                blackboard.hoverSelection = new((Vector3Int)CopySelectionMin, blackboard.selection.size);
                
                break;
        }

        blackboard.selectionOutlineActive = state is State.Selected or State.MovingSelection && !selectionCopied;
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

                EditorState.StartChangeBundle(changelogMessage);

                EditorState.SetTiles(originPositions.ToArray(), nullTiles, "Deleted original selection");
                EditorState.SetTiles(destinationPositions, originTiles, "Filled new selection");

                EditorState.EndChangeBundle();

                blackboard.selection.position += (Vector3Int)delta;

                break;
        }
    }

    public void Cut()
    {
        if (state != State.Selected) return;

        Copy();

        var nullTiles = new TileData[clipboardPositions.Count];
        EditorState.SetTiles(clipboardPositions.ToArray(), nullTiles, cutChangelogMessage);
    }
    
    public void Copy()
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
            clipboardTiles.Add(EditorState.GetTile(position));
        }
    }

    public void Paste()
    {
        if (!selectionCopied) return;
        
        Vector2Int delta = CopySelectionMin - clipboardAnchor;

        EditorState.SetTiles(
            clipboardPositions.Select(position => position + delta).ToArray(),
            clipboardTiles.ToArray(),
            pasteChangelogMessage);
    }
}
