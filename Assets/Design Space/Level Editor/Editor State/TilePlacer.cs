using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private Changelog changelog;
    [SerializeField] private Transform tilemapsParent;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private float wallThickness;
    [SerializeField] private float wallHeightBuffer;
    [SerializeField] private BoxCollider2D leftWall;
    [SerializeField] private BoxCollider2D rightWall;
    [SerializeField] private BoxCollider2D bottomHazard;
    [SerializeField] private TilePlacerReference tilePlacerReference;
    [SerializeField] private TileBase edgeTile;
    [SerializeField] private TileEditorState tileEditorState;

    private Dictionary<Tilemap, Tilemap> tilemapInstances;
    private Vector3Int[] previousEdgePositions;
    
    public Rect Rect => tileEditorState.WorldBounds;
    public RectInt RectInt => tileEditorState.Level.Bounds;
    
    private void Awake()
    {
        tilemapInstances = new();

        tilePlacerReference.value = this;
    }

    private void OnEnable()
    {
        changelog.ChangeEvent += OnEditorChanged;
    }

    private void OnDisable()
    {
        changelog.ChangeEvent -= OnEditorChanged;
    }

    public void PlaceTiles(Vector2Int[] positions, TileData[] tiles)
    {
        var tilemapInstanceData = new Dictionary<Tilemap, (List<Vector3Int> positions, List<TileBase> tiles)>();
        
        // Sort tiles based on tilemap prefab
        for (int i = 0; i < positions.Length; i++)
        {
            var tile = tiles[i];

            if (tile.gameTile == null || tile.gameTile.IsNullTileBase) continue;
                
            foreach (var tilemapInstance in tile.gameTile.TilemapPrefabs.Select(GetTilemapInstance))
            {
                if (!tilemapInstanceData.TryGetValue(tilemapInstance, out var tilemapData))
                {
                    tilemapData = (new(), new());
                    tilemapInstanceData.Add(tilemapInstance, tilemapData);
                }
                
                tilemapData.positions.Add((Vector3Int)positions[i]);
                tilemapData.tiles.Add(tile.gameTile.TileBase);
            }
        }

        // Clear drawing positions on all tilemaps
        var positions3 = positions.Select(position => (Vector3Int)position).ToArray();
        var nullTiles = new TileBase[positions.Length];
        
        foreach (var tilemap in tilemapInstances.Values)
        {
            tilemap.SetTiles(positions3, nullTiles);
        }
        
        // Set tiles
        foreach (var (tilemap, (vector3Ints, tileBases)) in tilemapInstanceData)
        {
            var tilemapPositions = vector3Ints.ToArray();
            var tilemapTiles = tileBases.ToArray();

            if (tilemap != null)
            {
                tilemap.SetTiles(tilemapPositions, tilemapTiles);
            }
        }
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case TileChangeInfo tileChangeInfo:

                ClearPreviousEdgeTiles();

                PlaceTiles(tileChangeInfo.positions, tileChangeInfo.newTiles);
                
                UpdateEdgeTiles();

                RefreshTilemaps();

                StartCoroutine(WaitFrame());
                IEnumerator WaitFrame()
                {
                    yield return null;
                    RefreshTilemaps();
                }
                
                break;
        }
    }

    private void RefreshTilemaps()
    {
        if (tilemapInstances.Count == 0) return;

        foreach (var tilemap in tilemapInstances.Values)
        {
            tilemap.CompressBounds();
            tilemap.CompressBounds();
            
            tilemap.RefreshAllTiles();

            if (tilemap.TryGetComponent(out CompositeCollider2D composite))
            {
                composite.GenerateGeometry();
            }
        }
        
        var rect = Rect;
        Vector2 wallSize = new(wallThickness, rect.size.y + wallHeightBuffer);
        
        leftWall.transform.position = new Vector2(rect.min.x - wallThickness / 2f, rect.center.y + wallHeightBuffer / 2f);
        leftWall.size = wallSize;

        rightWall.transform.position = new Vector2(rect.max.x + wallThickness / 2f, rect.center.y + wallHeightBuffer / 2f);
        rightWall.size = wallSize;

        bottomHazard.transform.position = new Vector2(rect.center.x, rect.min.y - wallThickness / 2f);
        bottomHazard.size = new(rect.size.x, wallThickness);
    }

    [SerializeField] private List<Tilemap> edgeOnTilemaps;
    
    private void UpdateEdgeTiles()
    {
        var rectInt = tileEditorState.Level.Bounds;
        
        // Fill edge tiles
        Vector2 size = rectInt.size + Vector2Int.one;
        var edgePositions = Enumerable.Range(0, (int)(size.x * 2 + size.y * 2 + 4))
            .Select(i => (Vector3Int)(rectInt.min - Vector2Int.one) + new Vector3Int(
                (int)Mathf.Clamp(size.x + size.y / 2 - Mathf.Abs(i - (size.y * 1.5f + size.x)), 0, size.x),
                (int)Mathf.Clamp(size.y + size.x / 2 - Mathf.Abs(i - (size.y + size.x / 2)), 0, size.y)))
            .ToArray();
        
        var edgeTiles = new TileBase[edgePositions.Length];
        Array.Fill(edgeTiles, edgeTile);
        
        foreach (var tilemap in tilemapInstances)
        {
            if (edgeOnTilemaps.Contains(tilemap.Key))
            {
                tilemap.Value.SetTiles(edgePositions, edgeTiles);
            }
        }

        previousEdgePositions = edgePositions;
    }

    private void ClearPreviousEdgeTiles()
    {
        if (previousEdgePositions != null)
        {
            var previousEdgeTiles = new TileBase[previousEdgePositions.Length];
            
            foreach (var tilemap in tilemapInstances)
            {
                if (edgeOnTilemaps.Contains(tilemap.Key))
                {
                    tilemap.Value.SetTiles(previousEdgePositions, previousEdgeTiles);
                }
            }
        }
    }

    private Tilemap GetTilemapInstance(Tilemap tilemapPrefab)
    {
        if (tilemapInstances.TryGetValue(tilemapPrefab, out var tilemapInstance))
        {
            return tilemapInstance;
        }
        
        tilemapInstance = Instantiate(tilemapPrefab, tilemapsParent);
        tilemapInstances[tilemapPrefab] = tilemapInstance;
    
        return tilemapInstance;
    }
}
