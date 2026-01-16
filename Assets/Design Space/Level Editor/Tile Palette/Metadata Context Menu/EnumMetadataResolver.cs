using System;
using System.Linq;
using UnityEngine;

public abstract class EnumMetadataResolver<T> : MetadataResolver where T : struct, Enum
{
    public override object Transform(string metadata) => Enum.Parse<T>(metadata);

    public override string[] GetOptions(TileEditorState tileEditorState) => Enum.GetNames(typeof(T));

    public override string GetCurrentValue(Vector2Int[] selection, TileEditorState tileEditorState)
    {
        var selectedEnums = selection
            .GroupBy(position => tileEditorState.LevelInstance.GetTileOnAnyLayer(position).GetMetaData<T>())
            .Select(group => group.Key)
            .Where(e => (int)(object)e != 0)
            .ToArray();

        string selected = selectedEnums.Length == 1
            ? selectedEnums.First().ToString()
            : "-";

        return selected;
    }
}