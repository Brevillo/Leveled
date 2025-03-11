using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = CreateMenuPath + "Brush", order = CreateMenuOrder)]
public class BrushToolAction : ToolbarAction
{
    private List<Vector2Int> brushedTiles = new();

    private bool inverseBrushMode;

    protected virtual GameTile DrawingTile => inverseBrushMode
        ? null
        : activeToolSide switch
        {
            ToolbarActionsManager.ToolSide.Primary => EditorState.PrimaryTile,
            ToolbarActionsManager.ToolSide.Secondary => EditorState.SecondaryTile,
            _ => null,
        };
    
    protected override void OnDown()
    {
        brushedTiles.Clear();
        
        inverseBrushMode = EditorState.GetTile(SpaceUtility.MouseCell).gameTile == EditorState.PrimaryTile;
    }

    protected override void OnPressed()
    {
        TilePlacer.PlaceTile(SpaceUtility.MouseCell, new(DrawingTile));
        brushedTiles.Add(SpaceUtility.MouseCell);
    }

    protected override void OnReleased()
    {
        if (DrawingTile != null && DrawingTile.Linkable)
        {
            LinkingGroupSetter.GetLinkingGroupAtMouse(linkingGroup => SetTiles(new(DrawingTile, linkingGroup)));
        }
        else
        {
            SetTiles(new(DrawingTile));
        }
    }
    
    private void SetTiles(TileData tileData)
    {
        var tiles = new TileData[brushedTiles.Count];
        Array.Fill(tiles, tileData);
        
        EditorState.SetTiles(brushedTiles.ToArray(), tiles, changelogMessage);
        
        brushedTiles.Clear();
    }
}
