using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

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
    [SerializeField] private List<Tilemap> edgeOnTilemaps;
    [SerializeField] private Color normalLayerColor;
    [SerializeField] private Color dimmedLayerColor;
    [SerializeField] private float layerColorChangeSpeed;
    [SerializeField] private LevelLayer levelLayerPrefab;

    private List<Layer> tilemapLayers;
    private Vector3Int[] previousEdgePositions;
    private int selectedLayer;

    private class Layer
    {
        private readonly LevelLayer parent;
        private readonly Dictionary<Tilemap, Tilemap> tilemapInstances;
        private readonly TilePlacer context;
        private readonly int layerID;
        
        private float dimPercent;

        public Layer(TilePlacer context, int layerID)
        {
            this.layerID = layerID;
            this.context = context;
            
            parent = Instantiate(context.levelLayerPrefab, context.tilemapsParent);
            parent.Initialize(layerID);

            tilemapInstances = new();
        }

        public void UpdateDimming()
        {
            float dimPercentTarget = context.selectedLayer <= 0 || context.selectedLayer == layerID ? 0f : 1f;
            float newDimPercent = Mathf.MoveTowards(dimPercent, dimPercentTarget,
                Time.deltaTime / context.layerColorChangeSpeed);

            if (newDimPercent == dimPercent) return;

            dimPercent = newDimPercent; 

            var color = Color.Lerp(context.normalLayerColor, context.dimmedLayerColor, dimPercent);
            
            foreach (var tilemap in AllInstances)
            {
                tilemap.color = color;
            }
        }
        
        public IEnumerable<Tilemap> AllInstances => tilemapInstances.Values;

        public IEnumerable<Tilemap> GetEdgedTilemaps(List<Tilemap> edgeOnTilemaps) => tilemapInstances
            .Where(kv => edgeOnTilemaps.Contains(kv.Key))
            .Select(kv => kv.Value);
        
        public Tilemap GetTilemapInstance(Tilemap tilemapPrefab)
        {
            // Try getting existing tilemap instance
            if (!tilemapInstances.TryGetValue(tilemapPrefab, out var tilemapInstance))
            {
                // Create new tilemap instance
                tilemapInstance = Instantiate(tilemapPrefab, parent.transform);
                tilemapInstances[tilemapPrefab] = tilemapInstance;
            }

            return tilemapInstance;
        }

        public void Destroy()
        {
            foreach (var tilemapInstance in AllInstances)
            {
                Object.Destroy(tilemapInstance.gameObject);
            }
            
            Object.Destroy(parent.gameObject);
        }
    }
    
    public Rect Rect => tileEditorState.WorldBounds;
    public RectInt RectInt => tileEditorState.LevelInstance.Bounds;

    private IEnumerable<Tilemap> AllTilemapInstances => tilemapLayers
        .SelectMany(layer => layer.AllInstances);

    private IEnumerable<Tilemap> EdgedTilemaps => tilemapLayers
        .SelectMany(layer => layer.GetEdgedTilemaps(edgeOnTilemaps));
    
    private void Awake()
    {
        tilemapLayers = new();

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

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case TileChangeInfo tileChangeInfo:

                PlaceTiles(tileChangeInfo.positions, tileChangeInfo.newTiles, tileChangeInfo.layerID);
            
                break;
        }
    }

    private void Update()
    {
        foreach (var layer in tilemapLayers)
        {
            layer.UpdateDimming();
        }
    }

    public void SetSelectedLayer(int layer)
    {
        selectedLayer = layer;
    }

    public void ResetTilemaps()
    {
        foreach (var layer in tilemapLayers)
        {
            layer.Destroy();
        }
        
        tilemapLayers.Clear();
    }
    
    public void PlaceTiles(Vector2Int[] positions, TileData[] tiles, int layerID = 0)
    {
        ClearPreviousEdgeTiles();

        PlaceTilesOnLayer(positions, tiles, GetLayer(layerID), layerID);
        
        UpdateEdgeTiles();
        RefreshTilemaps();
        UpdateWalls();
    }

    public void PlaceTilesLayerExclusive(Vector2Int[] positions, TileData[] tiles, int layerID = 0)
    {
        ClearPreviousEdgeTiles();

        var nullTiles = new TileData[positions.Length];

        var exclusiveLayer = GetLayer(layerID);
        
        foreach (var layer in tilemapLayers)
        {
            PlaceTilesOnLayer(positions, layer == exclusiveLayer ? tiles : nullTiles, layer, layerID);
        }
        
        UpdateEdgeTiles();
        RefreshTilemaps();
        UpdateWalls();
    }

    private Layer GetLayer(int layerID)
    {
        // Add needed layers
        while (tilemapLayers.Count <= layerID)
        {
            tilemapLayers.Add(new(this, tilemapLayers.Count));
        }

        return tilemapLayers[layerID];
    }
    
    private void PlaceTilesOnLayer(Vector2Int[] positions, TileData[] tiles, Layer layer, int layerID)
    {
        var tilemapInstanceData = new Dictionary<Tilemap, (List<Vector3Int> positions, List<TileBase> tiles, List<TileOrientation> rotations)>();

        // Sort tiles based on tilemap prefab
        for (int i = 0; i < positions.Length; i++)
        {
            var tile = tiles[i];

            if (tile.gameTile == null || tile.gameTile.IsNullTileBase) continue;
            
            foreach (var tilemapInstance in tile.gameTile.TilemapPrefabs.Select(layer.GetTilemapInstance))
            {
                if (!tilemapInstanceData.TryGetValue(tilemapInstance, out var tilemapData))
                {
                    tilemapData = (new(), new(), new());
                    tilemapInstanceData.Add(tilemapInstance, tilemapData);
                }

                tilemapData.positions.Add((Vector3Int)positions[i]);
                tilemapData.tiles.Add(tile.gameTile.TileBase);
                tilemapData.rotations.Add(tile.GetMetaData<EnumStruct<TileOrientation>>().value);
            }
        }

        // Clear drawing positions on all tilemaps
        var positions3 = positions.Select(position => (Vector3Int)position).ToArray();
        var nullTiles = new TileBase[positions.Length];
        
        foreach (var tilemap in layer.AllInstances)
        {
            tilemap.SetTiles(positions3, nullTiles);
        }
        
        // Set tiles
        foreach (var (tilemap, (vector2Ints, tileBases, tileRotations)) in tilemapInstanceData)
        {
            var tilemapPositions = vector2Ints.ToArray();
            var tilemapTiles = tileBases.ToArray();

            if (tilemap != null)
            {
                tilemap.SetTiles(tilemapPositions, tilemapTiles);
            }

            for (int i = 0; i < tileRotations.Count; i++)
            {
                if (tileRotations[i] == TileOrientation.Up) continue;

                var rotation = Quaternion.AngleAxis(tileRotations[i] switch
                {
                    TileOrientation.Down => 180f,
                    TileOrientation.Left => 90f,
                    TileOrientation.Right => -90f,
                    _ => 0f,
                }, Vector3.forward);
                
                tilemap.SetTransformMatrix(vector2Ints[i], Matrix4x4.Rotate(rotation));
            }
        }
    }
    
    private void RefreshTilemaps()
    {
        if (tilemapLayers.Count == 0) return;

        foreach (var tilemap in AllTilemapInstances)
        {
            tilemap.CompressBounds();
            tilemap.CompressBounds();
            
            tilemap.RefreshAllTiles();

            if (tilemap.TryGetComponent(out CompositeCollider2D composite))
            {
                composite.GenerateGeometry();
            }
        }
    }

    private void UpdateWalls()
    {
        var rect = Rect;
        Vector2 wallSize = new(wallThickness, rect.size.y + wallHeightBuffer);
        
        leftWall.transform.position = new Vector2(rect.min.x - wallThickness / 2f, rect.center.y + wallHeightBuffer / 2f);
        leftWall.size = wallSize;

        rightWall.transform.position = new Vector2(rect.max.x + wallThickness / 2f, rect.center.y + wallHeightBuffer / 2f);
        rightWall.size = wallSize;

        bottomHazard.transform.position = new Vector2(rect.center.x, rect.min.y - wallThickness / 2f);
        bottomHazard.size = new(rect.size.x, wallThickness);
    }
    
    private void UpdateEdgeTiles()
    {
        var rectInt = tileEditorState.LevelInstance.Bounds;
        
        // Get edge positions
        Vector2 size = rectInt.size + Vector2Int.one;
        var edgePositions = Enumerable.Range(0, (int)(size.x * 2 + size.y * 2 + 4))
            .Select(i => (Vector3Int)(rectInt.min - Vector2Int.one) + new Vector3Int(
                (int)Mathf.Clamp(size.x + size.y / 2 - Mathf.Abs(i - (size.y * 1.5f + size.x)), 0, size.x),
                (int)Mathf.Clamp(size.y + size.x / 2 - Mathf.Abs(i - (size.y + size.x / 2)), 0, size.y)))
            .ToArray();
        
        // Fill tilemaps
        var edgeTiles = new TileBase[edgePositions.Length];
        Array.Fill(edgeTiles, edgeTile);
        
        foreach (var tilemap in EdgedTilemaps)
        {
            tilemap.SetTiles(edgePositions, edgeTiles);
        }

        previousEdgePositions = edgePositions;
    }

    private void ClearPreviousEdgeTiles()
    {
        if (previousEdgePositions == null) return;
        
        var previousEdgeTiles = new TileBase[previousEdgePositions.Length];
            
        foreach (var tilemap in EdgedTilemaps)
        {
            tilemap.SetTiles(previousEdgePositions, previousEdgeTiles);
        }
    }
}
