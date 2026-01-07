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

    private Rect rect;
    private RectInt rectInt;
    private Dictionary<Tilemap, Tilemap> tilemapInstances;
    
    public Rect Rect => rect;
    public RectInt RectInt => rectInt;
    
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
        Array.Fill(nullTiles, null);
        
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
        rect = default;
        rectInt = default;
        
        if (tilemapInstances.Count == 0) return;
        
        rectInt = GetRectInt(tilemapInstances.Values.First());

        RectInt GetRectInt(Tilemap tilemap1)
        {
            return new RectInt((Vector2Int)tilemap1.cellBounds.position, (Vector2Int)tilemap1.cellBounds.size);
        }

        foreach (var tilemap in tilemapInstances.Values)
        {
            tilemap.CompressBounds();
            tilemap.CompressBounds();
            tilemap.RefreshAllTiles();

            if (tilemap.TryGetComponent(out CompositeCollider2D composite))
            {
                composite.GenerateGeometry();
            }

            var tilemapBounds = GetRectInt(tilemap);

            rectInt.SetMinMax(
                Vector2Int.Min(rectInt.min, tilemapBounds.min),
                Vector2Int.Max(rectInt.max, tilemapBounds.max));
        }

        rect = new Rect
        {
            size = rectInt.size,
            center = spaceUtility.Grid.transform.TransformPoint(rectInt.center),
        };
        
        Vector2 wallSize = new(wallThickness, rect.size.y + wallHeightBuffer);
        
        leftWall.transform.position = new Vector2(rect.min.x - wallThickness / 2f, rect.center.y + wallHeightBuffer / 2f);
        leftWall.size = wallSize;

        rightWall.transform.position = new Vector2(rect.max.x + wallThickness / 2f, rect.center.y + wallHeightBuffer / 2f);
        rightWall.size = wallSize;

        bottomHazard.transform.position = new Vector2(rect.center.x, rect.min.y - wallThickness / 2f);
        bottomHazard.size = new(rect.size.x, wallThickness);
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
