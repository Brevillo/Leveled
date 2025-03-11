using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private Transform tilemapsParent;
    [SerializeField] private Image tilemapBoundsOutline;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private float wallThickness;
    [SerializeField] private float wallHeightBuffer;
    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;
    [SerializeField] private BoxCollider2D bottomHazard;

    private Bounds bounds;
    private Dictionary<Tilemap, Tilemap> tilemaps;
    
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

    private void Update()
    {
        var outlineTransform = tilemapBoundsOutline.rectTransform;
        outlineTransform.position = spaceUtility.WorldToCanvas(bounds.center, outlineTransform);
        outlineTransform.sizeDelta = spaceUtility.WorldToCanvas(bounds.max, outlineTransform) -
                                     spaceUtility.WorldToCanvas(bounds.min, outlineTransform);
    }

    public void PlaceTile(Vector2Int position, TileData tile)
    {
        var selfTilemap = GetTilemap(tile);

        if (!tile.IsEmpty && selfTilemap != null)
        {
            selfTilemap.SetTile((Vector3Int)position, tile.gameTile.TileBase);
        }

        foreach (var tilemap in tilemaps.Values)
        {
            if (tilemap == selfTilemap) continue;
    
            tilemap.SetTile((Vector3Int)position, null);
        }
        
        CompressBounds();
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
        tilemapBoundsOutline.gameObject.SetActive(tilemaps.Count > 0);

        bounds = default;
        
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
