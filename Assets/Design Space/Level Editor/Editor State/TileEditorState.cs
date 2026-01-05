using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Tile Editor State")]
public class TileEditorState : GameService
{
    [SerializeField] private Changelog changelog;
    
    private readonly Dictionary<Vector2Int, TileData> tiles = new();

    public Dictionary<Vector2Int, TileData> EntityTiles =>
        new(tiles.Where(kv => !kv.Value.IsEmpty && kv.Value.gameTile.Entity != null));
    
    public IEnumerable<TileData> TileData => tiles.Values;

    public void SetTiles(Vector2Int[] positions, TileData[] tiles, string description) =>
        changelog.SendChange(new TileChangeInfo(positions, positions.Select(GetTile).ToArray(), tiles, description));

    public TileData GetTile(Vector2Int position) => 
        tiles.GetValueOrDefault(position);

    #region Changelog
    
    protected override void Initialize()
    {
        changelog.StateUpdated += OnStateUpdated;
    }

    protected override void InstanceDestroyed()
    {
        changelog.StateUpdated -= OnStateUpdated;
    }

    private void OnStateUpdated(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case TileChangeInfo tileChangeInfo:

                for (int i = 0; i < tileChangeInfo.positions.Length; i++)
                {
                    var tile = tileChangeInfo.newTiles[i];

                    Vector2Int position = tileChangeInfo.positions[i];
                    if (tile.IsEmpty)
                    {
                        tiles.Remove(position);
                    }
                    else
                    {
                        tiles[position] = tile;
                    }
                }

                break;
        }
    }
    
    #endregion
    
    #region Saving
    
    public void ClearAllTiles() => changelog.SendChange(new TileChangeInfo(
        tiles.Keys.ToArray(),
        tiles.Values.ToArray(),
        new TileData[tiles.Count],
        "Clear all tiles"));

    public LevelData LevelData => new()
    {
        positions = tiles.Keys.Select(vector => (SimpleVector2Int)vector).ToArray(),
        gameTileIds = tiles.Values.Select(data => data.gameTile.ID).ToArray(),
        linkingGroups = tiles.Values.Select(data => data.linkingGroup).ToArray(),
    };
    
    public void SetLevelData(LevelData levelData, GameTilePalette palette)
    {
        // Clear current level
        ClearAllTiles();
        
        // Fill new level
        SetTiles(
            levelData.positions.Select(simple => (Vector2Int)simple).ToArray(),
            Enumerable.Range(0, levelData.positions.Length)
                .Select(i => new TileData(palette.GetTile(levelData.gameTileIds[i]), levelData.linkingGroups[i]))
                .ToArray(),
            "Filled new level");
        
        // Clear changelog
        changelog.ClearChangelog();
    }
    
    #endregion
}
