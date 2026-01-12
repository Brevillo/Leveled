using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level
{
    private readonly Grid<TileData> baseLayer;
    private readonly Dictionary<Guid, Grid<TileData>> layers = new();
    private RectInt bounds;

    private static Grid<TileData> GetNewLayer() => 
        new DictionaryGrid();
    // new ResizableGrid<TileData>(Vector2Int.one * 1000, Vector2Int.one * -500);

    public void SetTile(Vector2Int position, TileData tileData, Guid layerID)
    {
        if (!layers.TryGetValue(layerID, out var layer))
        {
            layer = GetNewLayer();
            layers.Add(layerID, layer);
        }
        
        layer.SetTile(position, tileData);
    }

    public void UpdateBounds()
    {
        var allPositions = layers
            .SelectMany(item => item.Value.AllPositions)
            .ToArray();

        bounds = new()
        {
            min = allPositions.Aggregate(Vector2Int.one * int.MaxValue, Vector2Int.Min),
            max = allPositions.Aggregate(Vector2Int.one * int.MinValue, Vector2Int.Max) + Vector2Int.one,
        };
    }

    #region Getters
    
    public TileData GetTile(Vector2Int position)
    {
        foreach (var layer in layers.Values)
        {
            if (layer.TryGetTile(position, out var tileData))
            {
                return tileData;
            }
        }

        return default;
    }

    private IEnumerable<TileData> AllTiles => layers.Values.SelectMany(layer => layer.AllTiles);

    private IEnumerable<(Vector2Int position, TileData tileData)> AllTilePositions =>
        layers.Values.SelectMany(layer => layer.AllTilePositions);

    public IEnumerable<TMetadata> GetAllMetadata<TMetadata>() =>
        AllTiles.Select(tileData => tileData.GetMetaData<TMetadata>());

    public IEnumerable<Vector2Int> EntityPositions => AllTilePositions
        .Where(tile => tile.tileData.gameTile.HasEntity)
        .Select(tile => tile.position);
    
    #endregion

    public ChangeInfo ClearAllTilesChangeInfo => new ChangeInfoBundle("Clear all tiles", 
        layers
            .Select(kv =>
            {
                var positions = kv.Value.AllPositions.ToArray();
                
                return (ChangeInfo)new TileChangeInfo(
                    "Clear all tiles on layer",
                    kv.Key,
                    positions,
                    positions
                        .Select(position => kv.Value.TryGetTile(position, out var tileData)
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
            layers.AddRange(from layer in levelData.layers
                let gridSize = layer.gridSize
                let positions = layer.AllPositions
                select new TileChangeInfo("Filled layer",
                    layer.layerID,
                    positions,
                    new TileData[positions.Length],
                    TileData.GetTileData(layer, palette))
                );
        }

        if (levelData.positions != null)
        {
            layers.Add(new TileChangeInfo("Filled default layer",
                Guid.NewGuid(),
                levelData.positions.Select(SimpleVector2Int.ToVector2Int).ToArray(),
                new TileData[levelData.positions.Length],
                levelData.gameTileIds
                    .Select(gameTileID => new TileData(palette.GetTile(gameTileID)))
                    .ToArray()
                ));
        }

        return new ChangeInfoBundle("Constructed level state", layers.ToArray());
    }

    public LevelData GetLevelData() => new()
    {
        layers = layers
            .Select(layer => new LevelLayerData
            {
                layerID = layer.Key,
                gridSize = layer.Value.Rect.size + Vector2Int.one,
                gameTileIds = layer.Value.AllPositions
                    .Select(position => 
                        layer.Value.TryGetTile(position, out var tileData)
                            ? tileData.gameTile.ID
                            : default)
                    .ToArray(),
                metaData = new(layer.Value.AllPositions
                    .Select(position => new KeyValuePair<string, TileMetadata>(((SimpleVector2Int)position).ToString(), layer.Value.GetTileOrDefault(position).metadata))
                    .Where(metaData => metaData.Value != null && metaData.Value.Count != 0)),
            })
            .ToArray(),
    };
}