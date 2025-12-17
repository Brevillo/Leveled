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
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private float wallThickness;
    [SerializeField] private float wallHeightBuffer;
    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;
    [SerializeField] private BoxCollider2D bottomHazard;

    private Bounds bounds;
    private BoundsInt boundsInt;
    private Dictionary<Tilemap, Tilemap> tilemaps;
    
    public Bounds Bounds => bounds;
    public BoundsInt BoundsInt => boundsInt;
    
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

    public void PlaceTiles(Vector2Int[] positions, TileData[] tiles)
    {
        var tilemapGroups = Enumerable.Range(0, positions.Length)
            .Select(i =>
            {
                var tile = tiles[i];
                
                return (position: positions[i], tileData: tile, tilemap: GetTilemap(tile));
            })
            .GroupBy(item => item.tilemap)
            .ToArray();
        
        foreach (var group in tilemapGroups)
        {
            var positionArray = group.Select(item => (Vector3Int)item.position).ToArray();
            
            if (group.Key != null)
            {
                var tileArray = group.Select(item => item.tileData.gameTile.TileBase).ToArray();
                group.Key.SetTiles(positionArray, tileArray);
            }
            
            var nullTiles = new TileBase[positionArray.Length];
            Array.Fill(nullTiles, null);
            
            foreach (var tilemap in tilemaps.Values)
            {
                if (tilemap == group.Key) continue;
        
                tilemap.SetTiles(positionArray, nullTiles);
            }
        }
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case TileChangeInfo tileChangeInfo:
                
                PlaceTiles(tileChangeInfo.positions, tileChangeInfo.newTiles);
                
                CompressBounds();

                StartCoroutine(WaitFrame());
                IEnumerator WaitFrame()
                {
                    yield return null;
                    CompressBounds();
                }
                
                break;
        }
    }

    private void CompressBounds()
    {
        bounds = default;
        boundsInt = default;
        
        if (tilemaps.Count == 0) return;
        
        boundsInt = tilemaps.Values.First().cellBounds; 
        
        foreach (var tilemap in tilemaps.Values)
        {
            tilemap.CompressBounds();
            tilemap.CompressBounds();
            tilemap.RefreshAllTiles();

            if (tilemap.TryGetComponent(out CompositeCollider2D composite))
            {
                composite.GenerateGeometry();
            }

            var tilemapBounds = tilemap.cellBounds;

            boundsInt.SetMinMax(
                Vector3Int.Min(boundsInt.min, tilemapBounds.min),
                Vector3Int.Max(boundsInt.max, tilemapBounds.max));
        }

        bounds = new(spaceUtility.Grid.transform.TransformPoint(boundsInt.center), boundsInt.size);
        
        Vector2 wallSize = new(wallThickness, bounds.size.y + wallHeightBuffer); 
        
        leftWall.transform.position = new Vector2(bounds.min.x - wallThickness / 2f, bounds.center.y + wallHeightBuffer / 2f);
        leftWall.size = wallSize;

        rightWall.transform.position = new Vector2(bounds.max.x + wallThickness / 2f, bounds.center.y + wallHeightBuffer / 2f);
        rightWall.size = wallSize;

        bottomHazard.transform.position = new Vector2(bounds.center.x, bounds.min.y - wallThickness / 2f);
        bottomHazard.size = new(bounds.size.x, wallThickness);
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
