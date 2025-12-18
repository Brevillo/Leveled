using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = CreateMenuPath + "Fill")]
public class FillToolAction : ToolbarAction
{
    [SerializeField] private int maxIterations;
    
    protected override void OnDown()
    {
        var tile = activeToolSide switch
        {
            ToolSide.Primary => EditorState.PrimaryTile,
            ToolSide.Secondary => EditorState.SecondaryTile,
            _ => null,
        };

        var fillPositions = GetFloodFillPositions(SpaceUtility.MouseCell);
        
        var tiles = new TileData[fillPositions.Length];
        Array.Fill(tiles, new(tile));
        
        TilePlacer.PlaceTiles(fillPositions, tiles);

        if (tile != null && tile.Linkable)
        {
            LinkingGroupSetter.GetLinkingGroupAtMouse(linkingGroup =>
            {
                var linkedTiles = new TileData[fillPositions.Length];
                Array.Fill(linkedTiles, new(tile, linkingGroup));
                
                EditorState.SetTiles(fillPositions, linkedTiles, changelogMessage);
            });
        }
        else
        {
            EditorState.SetTiles(fillPositions, tiles, changelogMessage);
        }
    }

    private Vector2Int[] GetFloodFillPositions(Vector2Int start)
    {
        var bounds = TilePlacer.BoundsInt;
        
        GameTile fillingTile = EditorState.GetTile(start).gameTile;
        Queue<Vector2Int> checks = new();
        HashSet<Vector2Int> positions = new();

        checks.Enqueue(start);

        int iterations = 0;

        while (checks.Count > 0)
        {
            iterations++;
            if (iterations > maxIterations) break;
            
            Vector2Int position = checks.Dequeue();

            if (!bounds.Contains((Vector3Int)position) ||
                positions.Contains(position) || 
                EditorState.GetTile(position).gameTile != fillingTile)
            {
                continue;
            }
            
            positions.Add(position);
            
            checks.Enqueue(position + Vector2Int.right);
            checks.Enqueue(position + Vector2Int.left);
            checks.Enqueue(position + Vector2Int.up);
            checks.Enqueue(position + Vector2Int.down);
        }

        return positions.ToArray();
    }
}
