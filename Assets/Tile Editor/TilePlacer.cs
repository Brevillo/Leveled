using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private Transform tilemapsParent;
    [SerializeField] private SpriteRenderer tilemapBoundsOutline;
    [SerializeField] private float wallThickness;
    [SerializeField] private float wallHeightBuffer;
    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;
    [SerializeField] private BoxCollider2D bottomHazard;

    private Bounds bounds;
    private Dictionary<Tilemap, Tilemap> tilemaps;

    public Tilemap[] Tilemaps => tilemaps.Values.ToArray();
    public Bounds Bounds => bounds;
    
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

    public void PlaceTile(Vector3Int position, TileData tile)
    {
        var selfTilemap = GetTilemap(tile);

        if (!tile.IsEmpty && selfTilemap != null)
        {
            selfTilemap.SetTile(position, tile.gameTile.TileBase);
        }

        foreach (var tilemap in tilemaps.Values)
        {
            if (tilemap == selfTilemap) continue;
    
            tilemap.SetTile(position, null);
        }
        
        CompressBounds();
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case MultiTileChangeInfo multiTileChangeInfo:
                
                var tilemapGroups = Enumerable.Range(0, multiTileChangeInfo.positions.Length)
                    .Select(i =>
                    {
                        var tile = multiTileChangeInfo.newTiles[i];
                        
                        return (position: multiTileChangeInfo.positions[i], tileData: tile, tilemap: GetTilemap(tile));
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
                        var tileArray = group.Select(item => item.tileData.gameTile.TileBase).ToArray();
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

        CompressBounds();

        StartCoroutine(WaitFrame());
        IEnumerator WaitFrame()
        {
            yield return null;
            CompressBounds();
        }
    }

    private void CompressBounds()
    {
        tilemapBoundsOutline.gameObject.SetActive(tilemaps.Count > 0);
        if (tilemaps.Count == 0) return;
        
        bounds = WorldBounds(tilemaps.Values.First()); 
        
        foreach (var tilemap in tilemaps.Values)
        {
            tilemap.CompressBounds();
            tilemap.CompressBounds();
            tilemap.RefreshAllTiles();

            if (tilemap.TryGetComponent(out CompositeCollider2D composite))
            {
                composite.GenerateGeometry();
            }
            
            bounds.Encapsulate(WorldBounds(tilemap));
        }

        tilemapBoundsOutline.transform.position = bounds.center;
        tilemapBoundsOutline.size = bounds.size;

        Vector2 wallSize = new(wallThickness, bounds.size.y + wallHeightBuffer); 
        
        leftWall.transform.position = new Vector2(bounds.min.x - wallThickness / 2f, bounds.center.y + wallHeightBuffer / 2f);
        leftWall.size = wallSize;

        rightWall.transform.position = new Vector2(bounds.max.x + wallThickness / 2f, bounds.center.y + wallHeightBuffer / 2f);
        rightWall.size = wallSize;

        bottomHazard.transform.position = new Vector2(bounds.center.x, bounds.min.y - wallThickness / 2f);
        bottomHazard.size = new(bounds.size.x, wallThickness);
    }

    private static Bounds WorldBounds(Tilemap tilemap)
    {
        var localBounds = tilemap.localBounds;
        return new(tilemap.transform.TransformPoint(localBounds.center), localBounds.size);
    }
    
    private Tilemap GetTilemap(TileData tileData)
    {
        if (tileData.IsEmpty || tileData.gameTile.TilemapPrefab == null)
        {
            return null;
        }

        var tilemapPrefab = tileData.gameTile.TilemapPrefab;
        
        if (tilemaps.TryGetValue(tilemapPrefab, out var tilemap))
        {
            return tilemap;
        }
        
        tilemap = Instantiate(tilemapPrefab, tilemapsParent);
        tilemaps[tilemapPrefab] = tilemap;
    
        return tilemap;
    }
}
