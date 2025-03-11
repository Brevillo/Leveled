using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = CreateMenuPath + "Linker", order = CreateMenuOrder)]
public class LinkerToolAction : ToolbarAction
{
    protected override void OnReleased()
    {
        var selection = GetCurrentSelection();
        
        var boundsIntAnchorWorld = SpaceUtility.GetBoundsIntAnchorWorld(selection, new Vector2(0.5f, 0f));
        
        LinkingGroupSetter.GetLinkingGroup(
            boundsIntAnchorWorld,
            linkingGroup =>
            {
                var modifiedPositions = new List<Vector2Int>();
                var modifiedTiles = new List<TileData>();
                
                foreach (Vector2Int position in selection.allPositionsWithin)
                {
                    var tile = EditorState.GetTile(position);

                    if (tile.Linkable)
                    {
                        modifiedPositions.Add(position);
                        modifiedTiles.Add(new(tile.gameTile, linkingGroup));
                    }
                }
                
                EditorState.SetTiles(modifiedPositions.ToArray(), modifiedTiles.ToArray(), "Changed tile linking");
            });
    }
}
