using System;

public readonly struct TileData : IEquatable<TileData>
{
    public readonly GameTile gameTile;
    public readonly string linkingGroup;

    public TileData(GameTile gameTile)
    {
        this.gameTile = gameTile;
        linkingGroup = string.Empty;
    }

    public TileData(GameTile gameTile, string linkingGroup)
    {
        this.gameTile = gameTile;
        this.linkingGroup = linkingGroup;
    }

    public bool IsEmpty => gameTile == null || gameTile.TileBase == null;

    public bool Linkable => gameTile != null && gameTile.Linkable;

    public static TileData Empty => default;

    public bool Equals(TileData other) => Equals((object)other);

    public static bool operator ==(TileData t1, TileData t2) => t1.Equals(t2);
    public static bool operator !=(TileData t1, TileData t2) => !t1.Equals(t2);

    public override bool Equals(object obj)
        => obj is TileData other 
           && gameTile == other.gameTile
           && linkingGroup == other.linkingGroup;

    public override int GetHashCode() => HashCode.Combine(gameTile, linkingGroup);
}
