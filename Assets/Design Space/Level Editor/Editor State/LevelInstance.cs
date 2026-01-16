using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelInstance
{
    private readonly List<Grid<TileData>> layers = new();
    private RectInt bounds;

    private static Grid<TileData> GetNewLayer() => 
        new DictionaryGrid();
    // new ResizableGrid<TileData>(Vector2Int.one * 1000, Vector2Int.one * -500);

    public int GetNewLayerID() => layers.Count;
    
    public bool UpdateLayers(int layerID)
    {
        if (layerID < 0)
        {
            Debug.LogError("Tried to put a tile on a negative layer!");
            return true;
        }

        while (layerID >= layers.Count)
        {
            layers.Add(GetNewLayer());
        }

        return false;
    }

    /// <summary>
    /// Run UpdateLayers before running SetTile, since for optimization it does no layer validation.
    /// </summary>
    public void SetTile(Vector2Int position, TileData tileData, int layerID)
    {
        layers[layerID].SetTile(position, tileData);
    }

    public void UpdateBounds()
    {
        var allPositions = Layers
            .SelectMany(item => item.AllPositions)
            .ToArray();

        bounds = new()
        {
            min = allPositions.Aggregate(Vector2Int.one * int.MaxValue, Vector2Int.Min),
            max = allPositions.Aggregate(Vector2Int.one * int.MinValue, Vector2Int.Max) + Vector2Int.one,
        };
    }

    #region Getters
    
    public TileData GetTileOnAnyLayer(Vector2Int position)
    {
        foreach (var layer in Layers)
        {
            if (layer.TryGetTile(position, out var tileData))
            {
                return tileData;
            }
        }

        return default;
    }

    public TileData GetTile(Vector2Int position, int layerID) => layerID >= 0 && layerID < layers.Count 
        ? layers[layerID].GetTileOrDefault(position)
        : default;

    public int GetLayerIDAt(Vector2Int position)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].TryGetTile(position, out var tileData) && !tileData.IsEmpty)
            {
                return i;
            }
        }

        return -1;
    }

    public RectInt GetLayerRect(int layerID) => layerID < 0 || layerID > layers.Count
        ? default 
        : layers[layerID].Rect;

    private IEnumerable<Grid<TileData>> Layers => layers;
    
    private IEnumerable<TileData> AllTiles => Layers.SelectMany(layer => layer.AllTiles);

    private IEnumerable<(Vector2Int position, TileData tileData)> AllTilePositions =>
        Layers.SelectMany(layer => layer.AllTilePositions);

    public IEnumerable<int> AllLayerIDs => Enumerable.Range(0, layers.Count);
    
    public IEnumerable<TMetadata> GetAllMetadata<TMetadata>() =>
        AllTiles.Select(tileData => tileData.GetMetaData<TMetadata>());

    public IEnumerable<Vector2Int> EntityPositions => AllTilePositions
        .Where(tile => tile.tileData.gameTile.HasEntity)
        .Select(tile => tile.position);
    
    #endregion

    public ChangeInfo ClearAllTilesChangeInfo => new ChangeInfoBundle(
        "Clear all tiles", 
        Layers.Select((layer, layerID) => 
            {
                var positions = layer.AllPositions.ToArray();
                
                return (ChangeInfo)new TileChangeInfo(
                    "Clear all tiles on layer",
                    layerID,
                    positions,
                    positions
                        .Select(position => layer.TryGetTile(position, out var tileData)
                            ? tileData
                            : default)
                        .ToArray(), 
                    new TileData[positions.Length]);
            })
            .ToArray()
    );

    public RectInt Bounds => bounds;

    public ChangeInfo SetLevelData(LevelData levelData, GameTilePalette palette)
    {
        var layers = new List<ChangeInfo>();

        if (levelData.layers != null)
        {
            layers.AddRange(levelData.layers
                .Where(layer => layer.gameTileIds.Length > 0)
                .Select(layerData => (data: layerData, positions: layerData.AllPositions))
                .Select((layer, index) => new TileChangeInfo(
                    description: "Filled layer",
                    layerID: index,
                    positions: layer.positions,
                    previousTiles: new TileData[layer.positions.Length],
                    newTiles: layer.positions
                        .Select((position, i) => new TileData(
                            palette.GetTile(layer.data.gameTileIds[i]),
                            layer.data.metaData?.FirstOrDefault(item => SimpleVector2Int.Parse(item.Key) == position).Value))
                        .ToArray())));
        }

        if (levelData.positions != null)
        {
            layers.Add(new TileChangeInfo(
                description: "Filled default layer",
                layerID: 0,
                positions: levelData.positions.Select(SimpleVector2Int.ToVector2Int).ToArray(),
                previousTiles: new TileData[levelData.positions.Length],
                newTiles: levelData.gameTileIds
                    .Select(gameTileID => new TileData(palette.GetTile(gameTileID)))
                    .ToArray()
                ));
        }

        return new ChangeInfoBundle("Constructed level state", layers.ToArray());
    }

    public LevelData GetLevelData() => new()
    {
        layers = Layers
            .Where(layer => layer.AllTiles.Any())
            .Select(layer => new LevelLayerData
            {
                gridSize = layer.Rect.size + Vector2Int.one,
                gameTileIds = layer.AllPositions
                    .Select(position => 
                        layer.TryGetTile(position, out var tileData)
                            ? tileData.gameTile.ID
                            : default)
                    .ToArray(),
                minPosition = layer.Rect.min, 
                metaData = new(layer.AllPositions
                    .Select(position => new KeyValuePair<string, TileMetadata>(((SimpleVector2Int)position).ToString(), layer.GetTileOrDefault(position).metadata))
                    .Where(metaData => metaData.Value != null && metaData.Value.Count != 0)),
            })
            .ToArray(),
    };
}