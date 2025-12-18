using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = CreateMenuPath + "Brush")]
public class BrushToolAction : ToolbarAction
{
    [SerializeField] private SoundEffect placedSound;
    
    private List<Vector2Int> brushedTiles = new();

    private Vector2Int previousMouseCell;

    protected virtual GameTile DrawingTile => activeToolSide switch
    {
        ToolSide.Primary => EditorState.PrimaryTile,
        ToolSide.Secondary => EditorState.SecondaryTile,
        _ => null,
    };
    
    protected override void OnDown()
    {
        brushedTiles.Clear();
        
        previousMouseCell = SpaceUtility.MouseCell;
        
        placedSound.Play();
    }

    protected override void OnPressed()
    {
        Vector2Int current = SpaceUtility.MouseCell;
        
        var positions = GetPointsOnLine(previousMouseCell, current);

        var tiles = new TileData[positions.Length];
        Array.Fill(tiles, new(DrawingTile));
        
        TilePlacer.PlaceTiles(positions, tiles);
        brushedTiles.AddRange(positions);
        
        if (positions.Length > 0 && current != previousMouseCell)
        {
            placedSound.Play();
        }
        
        previousMouseCell = current;
    }

    protected override void OnReleased()
    {
        if (DrawingTile != null && DrawingTile.Linkable)
        {
            var tile = DrawingTile;
            LinkingGroupSetter.GetLinkingGroupAtMouse(linkingGroup => SetTiles(new(tile, linkingGroup)));
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

    // Implemented based on https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
    private static Vector2Int[] GetPointsOnLine(Vector2Int start, Vector2Int end)
    {
        var result = new List<Vector2Int>();

        Vector2Int delta = new(
            Mathf.Abs(end.x - start.x),
            -Mathf.Abs(end.y - start.y));
        
        Vector2Int sign = new(
            (int)Mathf.Sign(end.x - start.x),
            (int)Mathf.Sign(end.y - start.y));
        
        int error = delta.x + delta.y;

        while (true)
        {
            result.Add(start);
            int e2 = 2 * error;

            if (e2 >= delta.y)
            {
                if (start.x == end.x) break;
                error += delta.y;
                start.x += sign.x;
            }
            
            if (e2 <= delta.x)
            {
                if (start.y == end.y) break;
                error += delta.x;
                start.y += sign.y;
            }
        }
        
        return result.ToArray();
    }
}
