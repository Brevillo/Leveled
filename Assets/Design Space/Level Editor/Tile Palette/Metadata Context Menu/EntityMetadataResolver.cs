using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Entity")]
public class EntityMetadataResolver : MetadataResolver
{
    [SerializeField] private List<GameTile> entityOptions;

    public GameTile[] GetOptions() => entityOptions.ToArray();
    
    public override object GetCurrentValue(Vector2Int[] selection, TileEditorState tileEditorState) =>
        selection
            .GroupBy(position =>
                tileEditorState.LevelInstance.GetTileOnAnyLayer(position).GetMetaData<GameTileID>().value)
            .Select(group => group.Key)
            .Where(gameTileID => gameTileID != 0)
            .DefaultIfEmpty(-1)
            .First();
}