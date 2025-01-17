using System.Linq;
using UnityEngine;

public abstract class ChangeInfo
{
    protected string description;

    public abstract ChangeInfo Reverted { get; }

    protected ChangeInfo(string description)
    {
        this.description = description;
    }

    public override string ToString() => description;
}

public class ChangeInfoBundle : ChangeInfo
{
    public readonly ChangeInfo[] changeInfos;
    
    public ChangeInfoBundle(string description, ChangeInfo[] changeInfos) : base(description)
    {
        this.changeInfos = changeInfos;
    }

    public override ChangeInfo Reverted => new ChangeInfoBundle(description, changeInfos.Reverse().Select(change => change.Reverted).ToArray());
}

public class MultiTileChangeInfo : ChangeInfo
{
    public readonly Vector3Int[] positions;
    public readonly TileData[] previousTiles;
    public readonly TileData[] newTiles;

    public MultiTileChangeInfo(Vector3Int[] positions, TileData[] previousTiles, TileData[] newTiles) : base("Set multiple tiles.")
    {
        this.positions = positions;
        this.previousTiles = previousTiles;
        this.newTiles = newTiles;
    }

    public override ChangeInfo Reverted => new MultiTileChangeInfo(positions, newTiles, previousTiles);
}

public class TileChangeInfo : ChangeInfo
{
    public readonly Vector3Int position;
    public readonly TileData previousTile;
    public readonly TileData newTile;

    public TileChangeInfo(Vector3Int position, TileData previousTile, TileData newTile) : base("Set a tile")
    {
        this.position = position;
        this.previousTile = previousTile;
        this.newTile = newTile;
    }

    public override ChangeInfo Reverted=> new TileChangeInfo(position, newTile, previousTile);
}

public class ToolbarChangeInfo : ChangeInfo
{
    public readonly ToolType previousTool;
    public readonly ToolType newTool;

    public ToolbarChangeInfo(ToolType previousTool, ToolType newTool) : base($"Changed active tool to {newTool.ToString()}")
    {
        this.previousTool = previousTool;
        this.newTool = newTool;
    }

    public override ChangeInfo Reverted => new ToolbarChangeInfo(newTool, previousTool);
}

public class PaletteChangeInfo : ChangeInfo
{
    public readonly GameTile previousTile;
    public readonly GameTile newTile;

    public PaletteChangeInfo(GameTile previousTile, GameTile newTile) : base(
        $"Changed active tile from {GameTile.NullableToString(previousTile)} to {GameTile.NullableToString(newTile)}")
    {
        this.previousTile = previousTile;
        this.newTile = newTile;
    }

    public override ChangeInfo Reverted => new PaletteChangeInfo(newTile, previousTile);
}
