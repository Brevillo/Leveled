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

public readonly struct TileData
{
    public readonly GameTile gameTile;
    public readonly Metadata metadata;

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

    public TileData(GameTile gameTile, Metadata metadata)
    {
        this.gameTile = gameTile;
        this.metadata = metadata;
    }

    public bool Linkable => gameTile != null && gameTile.Linkable;
    public bool IsEmpty => gameTile == null || gameTile.IsNullTileBase;

    public TileData SetMetaData(object entry)
    {
        var newMetadata = metadata?.GetCopy() ?? new();
        
        newMetadata.SetValue(entry);
        
        return new(gameTile, newMetadata);
    }
}
