using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Linking Group")]
public class LinkingGroupMetadataResolver : StringMetadataResolver
{
    [SerializeField] private string[] defaultOptions;
    
    public override string[] GetOptions(TileEditorState tileEditorState) => defaultOptions
        .Concat(tileEditorState.LevelInstance
            .GetAllMetadata<LinkingGroup>()
            .Select(group => group.groupID))
        .Where(group => !string.IsNullOrEmpty(group))
        .GroupBy(groupID => groupID)
        .Select(group => group.Key)
        .ToArray();

    public override object GetCurrentValue(Vector2Int[] selection, TileEditorState tileEditorState) =>
        selection
            .GroupBy(position =>
                tileEditorState.LevelInstance.GetTileOnAnyLayer(position).GetMetaData<LinkingGroup>().groupID)
            .Select(group => group.Key)
            .Where(groupID => !string.IsNullOrEmpty(groupID))
            .DefaultIfEmpty("-")
            .First();

    public override object Transform(object metadata) => new LinkingGroup((string)metadata);
}