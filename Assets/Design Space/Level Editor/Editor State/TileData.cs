using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct LinkingGroup
{
    public readonly string groupID;
    
    public LinkingGroup(string groupID)
    {
        this.groupID = groupID;
    }
}

public readonly struct TileData
{
    public readonly GameTile gameTile;
    public readonly TileMetaData metaData;

    public T GetMetaData<T>() => metaData == null ? default : metaData.GetValueOrDefault<T>();
    
    public TileData(GameTile gameTile, params object[] metaData)
    {
        this.gameTile = gameTile;
        this.metaData = new();

        foreach (var entry in metaData)
        {
            this.metaData.SetValue(entry);
        }
    }

    public TileData(GameTile gameTile, TileMetaData metaData)
    {
        this.gameTile = gameTile;
        this.metaData = metaData;
    }

    public bool Linkable => gameTile != null && gameTile.Linkable;
    public bool IsEmpty => gameTile == null || gameTile.IsNullTileBase;

    public static TileData[] GetTileData(LevelLayerData layer, GameTilePalette palette) => layer.AllPositions
        .Select((position, i) => new TileData(
            palette.GetTile(layer.gameTileIds[i]),
            layer.metaData?.FirstOrDefault(item => SimpleVector2Int.Parse(item.Key) == position).Value))
        .ToArray();
}
