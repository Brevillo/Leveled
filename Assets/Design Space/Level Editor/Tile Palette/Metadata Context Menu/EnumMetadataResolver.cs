using System;
using System.Linq;
using UnityEngine;

public abstract class EnumMetadataResolver<T> : StringMetadataResolver where T : struct, Enum
{
    public override object Transform(object metadata) => new EnumStruct<T>(Enum.Parse<T>((string)metadata));

    public override string[] GetOptions(TileEditorState tileEditorState) => Enum.GetNames(typeof(T));

    public override object GetCurrentValue(Vector2Int[] selection, TileEditorState tileEditorState)
    {
        var selectedEnums = selection
            .GroupBy(position => tileEditorState.LevelInstance.GetTileOnAnyLayer(position).GetMetaData<EnumStruct<T>>())
            .Select(group => group.Key)
            .ToArray();

        string selected 
            = selectedEnums.Length switch
            {
                0 => ((T)(object)0).ToString(),
                1 => selectedEnums.First().value.ToString(),
                _ => "-",
            };

        return selected;
    }
}