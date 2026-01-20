using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = CreateMenuPath + "Selection")]
public class SelectionToolAction : ToolbarAction
{
    [SerializeField] private string pasteChangelogMessage;
    [SerializeField] private string cutChangelogMessage;
    [SerializeField] private string selectionChangelogMessage;
    [SerializeField] private MetadataResolverUIManagerReference metadataResolverUIManagerReference;
    
    private State state;

    private Vector2Int clipboardAnchor;
    private List<Vector2Int> clipboardPositions;
    private List<TileData> clipboardTiles;

    private Vector2Int CopySelectionMin
    {
        get
        {
            Vector3 selectionCenterOffset = SpaceUtility.GetRectIntCenterWorld(blackboard.selection.Value);
            Vector3 worldPosition = SpaceUtility.MouseWorld + (Vector3)Vector2.one / 2f;
            Vector3 snappedWorldCenter = 
                SpaceUtility.SnapWorldToCell(worldPosition - selectionCenterOffset) + selectionCenterOffset;
            
            return SpaceUtility.WorldToCell(snappedWorldCenter - (Vector3)(Vector3Int)blackboard.selection.Value.size / 2f);
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

    protected override void OnDeactivated()
    {
        metadataResolverUIManagerReference.value.Close();
    }

    protected override void OnDown()
    {
        if (activeToolSide == ToolSide.Secondary && blackboard.selection.Value != default)
        {
            OpenMetadataEditor();
            return;
        }

        metadataResolverUIManagerReference.value.Close();

        switch (state)
        {
            case State.None:
                
                state = blackboard.selection.Value.Contains(dragStart)
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
                
                blackboard.hoverSelection = new(blackboard.selection.Value.position + (SpaceUtility.MouseCell - dragStart), blackboard.selection.Value.size);
                
                break;
            
            case State.Copied:
                
                blackboard.hoverSelection = new(CopySelectionMin, blackboard.selection.Value.size);
                
                break;
        }
    }

    protected override void OnReleased()
    {
        switch (state)
        {
            case State.Selecting:

                if (CurrentSelection.size == Vector2Int.one && blackboard.selection.Value != default)
                {
                    blackboard.Deselect();
                }
                else
                {
                    blackboard.selection.SetValue(CurrentSelection, "Created Selection");
                } 

                if (activeToolSide == ToolSide.Secondary)
                {
                    OpenMetadataEditor();
                }

                state = State.None;
                
                break;
                        
            case State.MovingSelection:

                state = State.None;

                if (dragStart == SpaceUtility.MouseCell) break;

                blackboard.changelog.StartChangeBundle(changelogMessage);

                var originPositions = blackboard.SelectionPositions.ToArray();
                var nullTiles = new TileData[originPositions.Length];

                Vector2Int delta = SpaceUtility.MouseCell - dragStart;
                var destinationPositions = originPositions
                    .Select(position => position + delta)
                    .ToArray();

                foreach (var layer in EditorState.LevelInstance.AllLayerIDs)
                {
                    var originTiles = originPositions
                        .Select(position => EditorState.LevelInstance.GetTile(position, layer))
                        .ToArray();

                    EditorState.SetTiles(originPositions.ToArray(), nullTiles, "Deleted original selection", layer);
                    EditorState.SetTiles(destinationPositions, originTiles, "Filled new selection", layer);
                }

                blackboard.selection.SetValue(new RectInt(blackboard.selection.Value.position + delta,
                    blackboard.selection.Value.size), "Moved Selection");

                blackboard.changelog.EndChangeBundle();

                break;
        }
    }

    public void OpenMetadataEditor()
    {
        if (blackboard.selection.Value.Contains(dragStart))
        {
            metadataResolverUIManagerReference.value.Open(blackboard.SelectionPositions.ToArray());
        }
        else if (blackboard.selection.Value == default)
        {
            metadataResolverUIManagerReference.value.Open(new[] { dragStart });
        }
        else
        {
            metadataResolverUIManagerReference.value.Close();
            state = State.None;
            blackboard.Deselect();
        }
    }
    
    public void Cut()
    {
        if (blackboard.selection == default) return;

        if (blackboard.activeTool.Value != this)
        {
            blackboard.activeTool.Value = this;
        }

        Copy();

        var nullTiles = new TileData[clipboardPositions.Count];
        EditorState.SetTiles(clipboardPositions.ToArray(), nullTiles, cutChangelogMessage);
    }
    
    public void Copy()
    {
        if (blackboard.selection == default) return;

        if (blackboard.activeTool.Value != this)
        {
            blackboard.activeTool.Value = this;
        }
        
        state = State.Copied;
        clipboardAnchor = blackboard.selection.Value.position;

        clipboardPositions ??= new();
        clipboardTiles ??= new();
        
        clipboardPositions.Clear();
        clipboardTiles.Clear();

        foreach (Vector2Int position in blackboard.selection.Value.allPositionsWithin)
        {
            clipboardPositions.Add(position);
            clipboardTiles.Add(EditorState.LevelInstance.GetTileOnAnyLayer(position));
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

    public void ResetLayer()
    {
        if (blackboard.selection == default) return;

        if (blackboard.activeTool.Value != this)
        {
            blackboard.activeTool.Value = this;
        }
        
        EditorState.MoveTilesToLayer(blackboard.SelectionPositions.ToArray(), 0);
    }

    public void NewLayer()
    {
        if (blackboard.selection == default) return;

        if (blackboard.activeTool.Value != this)
        {
            blackboard.activeTool.Value = this;
        }
        
        EditorState.MoveTilesToLayer(blackboard.SelectionPositions.ToArray(), EditorState.LevelInstance.GetNewLayerID());
    }
}
