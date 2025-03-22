using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = CreateMenuPath + "Selection", order = CreateMenuOrder)]
public class SelectionToolAction : ToolbarAction
{
    [SerializeField] private string pasteChangelogMessage;
    [SerializeField] private string cutChangelogMessage;
    [SerializeField] private string selectionChangelogMessage;
    
    private State state;

    private Vector2Int clipboardAnchor;
    private List<Vector2Int> clipboardPositions;
    private List<TileData> clipboardTiles;

    private Vector2Int CopySelectionMin
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
        MovingSelection,
        Copied,
    }

    protected override void OnActivated()
    {
        state = State.None;
    }

    protected override void OnDown()
    {
        if (activeToolSide == ToolSide.Secondary)
        {
            state = State.None;
            blackboard.Deselect();
            
            return;
        }

        switch (state)
        {
            case State.None:
                
                state = blackboard.selection.Contains((Vector3Int)dragStart)
                    ? State.MovingSelection
                    : State.Selecting;
                
                break;
            
            case State.Copied:

                Paste();

                break;
        }
    }

    protected override void OnUpdate()
    {
        switch (state)
        {
            case State.Selecting:

                blackboard.hoverSelection = CurrentSelection;
                
                break;
            
            case State.MovingSelection:
                
                blackboard.hoverSelection = new(blackboard.selection.position + (Vector3Int)(SpaceUtility.MouseCell - dragStart), blackboard.selection.size);
                
                break;
            
            case State.Copied:
                
                blackboard.hoverSelection = new((Vector3Int)CopySelectionMin, blackboard.selection.size);
                
                break;
        }
    }

    protected override void OnReleased()
    {
        switch (state)
        {
            case State.Selecting:

                blackboard.SetSelection(CurrentSelection, selectionChangelogMessage);

                state = State.None;
                        
                break;
                        
            case State.MovingSelection:

                state = State.None;

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

                blackboard.SetSelection(new BoundsInt(blackboard.selection.position + (Vector3Int)delta, blackboard.selection.size), "Moved selection");

                EditorState.EndChangeBundle();

                break;
        }
    }

    public void Cut()
    {
        if (blackboard.selection == default) return;

        if (EditorState.ActiveTool != this)
        {
            EditorState.ActiveTool = this;
        }

        Copy();

        var nullTiles = new TileData[clipboardPositions.Count];
        EditorState.SetTiles(clipboardPositions.ToArray(), nullTiles, cutChangelogMessage);
    }
    
    public void Copy()
    {
        if (blackboard.selection == default) return;

        if (EditorState.ActiveTool != this)
        {
            EditorState.ActiveTool = this;
        }
        
        state = State.Copied;
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
        if (state != State.Copied) return;
        
        Vector2Int delta = CopySelectionMin - clipboardAnchor;

        EditorState.SetTiles(
            clipboardPositions.Select(position => position + delta).ToArray(),
            clipboardTiles.ToArray(),
            pasteChangelogMessage);
    }
}
