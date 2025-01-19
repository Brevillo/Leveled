using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private Grid grid;

    private Dictionary<Tilemap, Tilemap> tilemaps;

    public Tilemap[] Tilemaps => tilemaps.Values.ToArray();
    
    private void Awake()
    {
        tilemaps = new();
    }

    private void OnEnable()
    {
        editorState.EditorChanged += OnEditorChanged;
    }

    private void OnDisable()
    {
        editorState.EditorChanged -= OnEditorChanged;
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case TileChangeInfo tileChangeInfo:

                var position = tileChangeInfo.position;
                var tile = tileChangeInfo.newTile;
                var selfTilemap = GetTilemap(tile);
        
                if (tile != null)
                {
                    selfTilemap.SetTile(position, tile.TileBase);
                }
        
                foreach (var tilemap in tilemaps.Values)
                {
                    if (tilemap == selfTilemap) continue;
            
                    tilemap.SetTile(position, null);
                }
                
                break;
            
            case MultiTileChangeInfo multiTileChangeInfo:
                
                var tilemapGroups = Enumerable.Range(0, multiTileChangeInfo.positions.Length)
                    .Select(i =>
                    {
                        var position = multiTileChangeInfo.positions[i];
                        var tile = multiTileChangeInfo.newTiles[i];
                        var tilemap = GetTilemap(tile);
                        
                        return (position, tile, tilemap);
                    })
                    .GroupBy(item => item.tilemap)
                    .ToArray();
                
                foreach (var group in tilemapGroups)
                {
                    var positionArray = group.Select(item => item.position).ToArray();
                    var nullTiles = new TileBase[positionArray.Length];
                    Array.Fill(nullTiles, null);
                    
                    if (group.Key != null)
                    {
                        var tileArray = group.Select(item => item.tile.TileBase).ToArray();
                        group.Key.SetTiles(positionArray, tileArray);
                    }
                    
                    foreach (var tilemap in tilemaps.Values)
                    {
                        if (tilemap == group.Key) continue;
                
                        tilemap.SetTiles(positionArray, nullTiles);
                    }
                }
                
                break;
        }
        
        foreach (var tilemap in tilemaps.Values)
        {
            tilemap.CompressBounds();
        }
    }

    private Tilemap GetTilemap(GameTile gameTile)
    {
        if (gameTile == null || gameTile.TilemapPrefab == null)
        {
            return null;
        }
    
        if (tilemaps.TryGetValue(gameTile.TilemapPrefab, out var tilemap))
        {
            return tilemap;
        }
        
        tilemap = Instantiate(gameTile.TilemapPrefab, grid.transform);
        tilemaps[gameTile.TilemapPrefab] = tilemap;
    
        return tilemap;
    }
}
