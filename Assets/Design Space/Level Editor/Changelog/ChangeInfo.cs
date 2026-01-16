using System;
using System.Linq;
using UnityEngine;

public abstract class ChangeInfo
{
    protected readonly string description;

    public abstract ChangeInfo Reverted { get; }

    protected ChangeInfo(string description)
    {
        this.description = description;
    }

    public override string ToString() => description;
}

public class ValueChangeInfo<T> : ChangeInfo
{
    public readonly string name;
    public readonly T previousValue;
    public readonly T newValue;

    public ValueChangeInfo(string name, T previousValue, T newValue, string description) : base(description)
    {
        this.name = name;
        this.previousValue = previousValue;
        this.newValue = newValue;
    }

    public override ChangeInfo Reverted => new ValueChangeInfo<T>(name, newValue, previousValue, description);
}

public class ChangeInfoBundle : ChangeInfo
{
    public readonly ChangeInfo[] changeInfos;
    
    public ChangeInfoBundle(string description, ChangeInfo[] changeInfos) : base(description)
    {
        this.changeInfos = changeInfos;
    }

    public override ChangeInfo Reverted => 
        new ChangeInfoBundle(description, changeInfos.Reverse().Select(change => change.Reverted).ToArray());
}

public class TileChangeInfo : ChangeInfo
{
    public readonly int layerID;
    public readonly Vector2Int[] positions;
    public readonly TileData[] previousTiles;
    public readonly TileData[] newTiles;

    public TileChangeInfo(
        string description, 
        int layerID,
        Vector2Int[] positions,
        TileData[] previousTiles,
        TileData[] newTiles)
        : base(description)
    {
        this.layerID = layerID;
        this.positions = positions;
        this.previousTiles = previousTiles;
        this.newTiles = newTiles;
    }

    public override ChangeInfo Reverted => new TileChangeInfo(description, layerID, positions, newTiles, previousTiles);
}