using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = CreateMenuPath + "Fill")]
public class FillToolAction : ToolbarAction
{
    [SerializeField] private int maxIterations;
    [SerializeField] private SoundEffect placedSound;

    protected override void OnDown()
    {
        var fillPositions = GetFloodFillPositions(SpaceUtility.MouseCell);
        
        var tiles = new TileData[fillPositions.Length];
        Array.Fill(tiles, new(DrawingTile));
        
        TilePlacer.PlaceTiles(fillPositions, tiles);

        if (DrawingTile != null && DrawingTile.Linkable)
        {
            LinkingGroupSetter.GetLinkingGroupAtMouse(linkingGroup =>
            {
                var linkedTiles = new TileData[fillPositions.Length];
                Array.Fill(linkedTiles, new(DrawingTile, linkingGroup));
                
                EditorState.SetTiles(fillPositions, linkedTiles, changelogMessage);
            });
        }
        else
        {
            EditorState.SetTiles(fillPositions, tiles, changelogMessage);
        }
        
        placedSound.Play();
    }

    private Vector2Int[] GetFloodFillPositions(Vector2Int start)
    {
        var bounds = TilePlacer.RectInt;
        
        GameTile fillingTile = EditorState.LevelInstance.GetTileOnAnyLayer(start).gameTile;
        Queue<Vector2Int> checks = new();
        HashSet<Vector2Int> positions = new();

        checks.Enqueue(start);

        int iterations = 0;

        while (checks.Count > 0)
        {
            iterations++;
            if (iterations > maxIterations) break;
            
            Vector2Int position = checks.Dequeue();

            if (!bounds.Contains(position) ||
                positions.Contains(position) || 
                EditorState.LevelInstance.GetTileOnAnyLayer(position).gameTile != fillingTile)
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
