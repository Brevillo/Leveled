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

public enum TileRotation
{
    Up,
    Down,
    Left,
    Right,
}

public readonly struct SignContents
{
    public readonly string text;
}

public readonly struct TileData
{
    public readonly GameTile gameTile;
    public readonly TileMetadata metadata;

    public T GetMetaData<T>() => metadata == null ? default : metadata.GetValueOrDefault<T>();
    
    public TileData(GameTile gameTile, params object[] metaData)
    {
        this.gameTile = gameTile;
        this.metadata = new();

        foreach (var entry in metaData)
        {
            this.metadata.SetValue(entry);
        }
    }

    public TileData(GameTile gameTile, TileMetadata metadata)
    {
        this.gameTile = gameTile;
        this.metadata = metadata;
    }

    public bool Linkable => gameTile != null && gameTile.Linkable;
    public bool IsEmpty => gameTile == null || gameTile.IsNullTileBase;

    public TileData SetMetaData(object entry)
    {
        var newMetadata = metadata ?? new();
        
        newMetadata.SetValue(entry);
        
        return new(gameTile, newMetadata);
    }
    
    public static TileData[] GetTileData(LevelLayerData layer, GameTilePalette palette) => layer.AllPositions
        .Select((position, i) => new TileData(
            palette.GetTile(layer.gameTileIds[i]),
            layer.metaData?.FirstOrDefault(item => SimpleVector2Int.Parse(item.Key) == position).Value))
        .ToArray();
}
