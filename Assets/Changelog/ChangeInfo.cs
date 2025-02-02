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

public abstract class ValueChangeInfo<T> : ChangeInfo
{
    public readonly T previousValue;
    public readonly T newValue;

    public ValueChangeInfo(T previousValue, T newValue, string description) : base(description)
    {
        this.previousValue = previousValue;
        this.newValue = newValue;
    }
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

public class MultiTileChangeInfo : ChangeInfo
{
    public readonly Vector3Int[] positions;
    public readonly TileData[] previousTiles;
    public readonly TileData[] newTiles;

    public MultiTileChangeInfo(Vector3Int[] positions, TileData[] previousTiles, TileData[] newTiles) : base(
        "Set multiple tiles.")
    {
        this.positions = positions;
        this.previousTiles = previousTiles;
        this.newTiles = newTiles;
    }

    public override ChangeInfo Reverted => new MultiTileChangeInfo(positions, newTiles, previousTiles);
}

public class ToolbarChangeInfo : ValueChangeInfo<ToolType>
{
    public ToolbarChangeInfo(ToolType previousValue, ToolType newValue) : base(previousValue, newValue,
        $"Changed active tool to {newValue.ToString()}")
    {
    }

    public override ChangeInfo Reverted => new ToolbarChangeInfo(newValue, previousValue);
}

public class PaletteChangeInfo : ValueChangeInfo<GameTile>
{
    public enum Type
    {
        Primary,
        Secondary,
    }

    public readonly Type type; 
    
    public PaletteChangeInfo(GameTile previousValue, GameTile newValue, Type type) : base(previousValue, newValue,
        $"Changed {type} tile from {GameTile.NullableToString(previousValue)} to {GameTile.NullableToString(newValue)}")
    {
        this.type = type;
    }

    public override ChangeInfo Reverted => new PaletteChangeInfo(newValue, previousValue, type);
}

public class ShowLinkingGroupsChangeInfo : ValueChangeInfo<bool>
{
    public ShowLinkingGroupsChangeInfo(bool previousValue, bool newValue) : base(previousValue, newValue,
        $"Changed 'Show Linking Group' from {previousValue} to {newValue}")
    {
    }

    public override ChangeInfo Reverted => new ShowLinkingGroupsChangeInfo(newValue, previousValue);
}

public class ShowPlayerPositionRecordingChangeInfo : ValueChangeInfo<bool>
{
    public ShowPlayerPositionRecordingChangeInfo(bool previousValue, bool newValue) : base(previousValue, newValue,
        $"Changed 'Show Player Position Recording' from {previousValue} to {newValue}")
    {
    }

    public override ChangeInfo Reverted => new ShowPlayerPositionRecordingChangeInfo(newValue, previousValue);
}