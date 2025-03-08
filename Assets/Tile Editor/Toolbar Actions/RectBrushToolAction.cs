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
            ToolbarActionsManager.ToolSide.Primary => EditorState.PrimaryTile,
            ToolbarActionsManager.ToolSide.Secondary => EditorState.SecondaryTile,
            _ => null
        };
            
        Vector3Int dragEnd = SpaceUtility.MouseCell;
        Vector3Int min = Vector3Int.Min(dragStart, dragEnd);
        Vector3Int max = Vector3Int.Max(dragStart, dragEnd);
        Vector3Int size = max - min + new Vector3Int(1, 1, 0);

        var positions = Enumerable.Range(0, size.x * size.y)
            .Select(i => new Vector3Int(min.x + i % size.x, min.y + i / size.x))
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
                
                EditorState.SetTiles(positions, linkedTiles, "Brushed tile rectangle");
            });
        }
        else
        {
            EditorState.SetTiles(positions, tiles, "Brushed tile rectangle");
        }

    }
}
