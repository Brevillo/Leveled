using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Linking Group")]
public class LinkingGroupMetadataResolver : MetadataResolver
{
    [SerializeField] private string[] defaultOptions;
    
    public override string[] GetOptions(TileEditorState tileEditorState) => defaultOptions
        .Concat(tileEditorState.Level
            .GetAllMetadata<LinkingGroup>()
            .Select(group => group.groupID))
        .Where(group => !string.IsNullOrEmpty(group))
        .GroupBy(groupID => groupID)
        .Select(group => group.Key)
        .ToArray();

    public override string GetCurrentValue(Vector2Int[] selection, TileEditorState tileEditorState)
    {
        var selectedGroupIDs = selection
            .GroupBy(position => tileEditorState.Level.GetTile(position).GetMetaData<LinkingGroup>().groupID)
            .Select(group => group.Key)
            .Where(groupID => !string.IsNullOrEmpty(groupID))
            .ToArray();

        string selected = selectedGroupIDs.Length == 1
            ? selectedGroupIDs.First()
            : "-";

        return selected;
    }
    
    public override object Transform(string metadata) => new LinkingGroup(metadata);
}