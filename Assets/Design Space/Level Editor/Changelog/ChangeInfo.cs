using System;
using System.Linq;
using UnityEngine;

public abstract class ChangeInfo
{
    public readonly string description;

    public abstract ChangeInfo Reverted { get; }

    protected ChangeInfo(string description)
    {
        this.description = description;
    }
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

public class LayerMetadataChangeInfo : ChangeInfo
{
    public enum Type
    {
        Add,
        Remove,
    }

    public readonly int layerID;
    public readonly object metadataValue;
    public readonly Type type;
    
    public LayerMetadataChangeInfo(string description, int layerID, object metadataValue, Type type) : base(description)
    {
        this.layerID = layerID;
        this.metadataValue = metadataValue;
        this.type = type;
    }

    public override ChangeInfo Reverted => new LayerMetadataChangeInfo(description, layerID, metadataValue, type switch
    {
        Type.Add => Type.Remove,
        Type.Remove => Type.Add,
        _ => throw new ArgumentOutOfRangeException(),
    });
}
