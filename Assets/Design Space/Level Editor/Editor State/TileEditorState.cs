using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelState
{
    private Dictionary<Vector2Int, TileData> tiles = new();
    
    public Vector2Int[] Positions => tiles.Keys.ToArray();
    public TileData[] TileData => tiles.Values.ToArray();
    
    public Dictionary<Vector2Int, TileData> EntityTiles => new(tiles.Where(kv => !kv.Value.IsEmpty && kv.Value.gameTile.Entity != null));
    
    public TileData[] NullTiles => new TileData[tiles.Count];

    public void SetTile(Vector2Int position, TileData tileData)
    {
        if (tileData.IsEmpty)
        {
            tiles.Remove(position);
        }
        else
        {
            tiles[position] = tileData;
        }
    }

    public TileData GetTile(Vector2Int position) => tiles.GetValueOrDefault(position);

    public void ClearTiles()
    {
        tiles.Clear();
    }
}

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Tile Editor State")]
public class TileEditorState : GameService
{
    [SerializeField] private Changelog changelog;

    private readonly LevelState level = new LevelState();

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

                    Level.SetTile(tileChangeInfo.positions[i], tile);
                }

                break;
        }
    }

    public void SetTiles(Vector2Int[] positions, TileData[] tiles, string description) =>
        changelog.SendChange(new TileChangeInfo(positions, positions.Select(Level.GetTile).ToArray(), tiles, description));

    public LevelState Level => level;
    
    public LevelData GetLevelData() => new()
    {
        positions = Level.Positions.Select(vector => (SimpleVector2Int)vector).ToArray(),
        gameTileIds = Level.TileData.Select(data => data.gameTile.ID).ToArray(),
        linkingGroups = Level.TileData.Select(data => data.linkingGroup).ToArray(),
    };

    public void ClearAllTiles()
    {
        // Clear Tiles
        SetTiles(Level.Positions, Level.NullTiles, "Cleared all tiles");
        
        // Reset editor state
        changelog.ClearChangelog();
        Level.ClearTiles();
    }
    
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
}
