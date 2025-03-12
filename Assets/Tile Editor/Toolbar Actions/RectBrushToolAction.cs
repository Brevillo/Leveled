using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = CreateMenuPath + "Rect Brush", order = CreateMenuOrder)]
public class RectBrushToolAction : ToolbarAction
{
    protected override void OnPressed()
    {
        blackboard.hoverSelectionCenter = SelectionCenter;
        blackboard.hoverSelectionSize = SelectionSize;
    }

    protected override void OnReleased()
    {
        GameTile tile = activeToolSide switch
        {
            ToolSide.Primary => EditorState.PrimaryTile,
            ToolSide.Secondary => EditorState.SecondaryTile,
            _ => null,
        };
            
        Vector2Int dragEnd = SpaceUtility.MouseCell;
        Vector2Int min = Vector2Int.Min(dragStart, dragEnd);
        Vector2Int max = Vector2Int.Max(dragStart, dragEnd);
        Vector2Int size = max - min + Vector2Int.one;

        var positions = Enumerable.Range(0, size.x * size.y)
            .Select(i => new Vector2Int(min.x + i % size.x, min.y + i / size.x))
            .ToArray();

        var tiles = new TileData[positions.Length];
        Array.Fill(tiles, new(tile));
    
        if (tile != null && tile.Linkable)
        {
            TilePlacer.PlaceTiles(positions, tiles);
            
            LinkingGroupSetter.GetLinkingGroupAtMouse(linkingGroup =>
            {
                var linkedTiles = new TileData[positions.Length];
                Array.Fill(linkedTiles, new(tile, linkingGroup));
                
                EditorState.SetTiles(positions, linkedTiles, changelogMessage);
            });
        }
        else
        {
            EditorState.SetTiles(positions, tiles, changelogMessage);
        }

    }
}
