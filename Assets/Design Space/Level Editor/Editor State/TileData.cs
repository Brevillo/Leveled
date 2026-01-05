using System;
using System.Collections.Generic;

public readonly struct TileData
{
    public readonly GameTile gameTile;
    public readonly string linkingGroup;

    public readonly Dictionary<string, object> customData;

    public TileData(GameTile gameTile)
    {
        this.gameTile = gameTile;
        linkingGroup = string.Empty;
        customData = new();
    }

    public TileData(GameTile gameTile, string linkingGroup)
    {
        this.gameTile = gameTile;
        this.linkingGroup = linkingGroup;
        customData = new();
    }

    public bool Linkable => gameTile != null && gameTile.Linkable;

    public bool IsEmpty => gameTile == null || gameTile.IsNullTileBase;
    
    public static TileData Empty => default;
}
