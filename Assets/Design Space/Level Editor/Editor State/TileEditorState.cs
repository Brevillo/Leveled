using System;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Tile Editor State")]
public class TileEditorState : GameService
{
    [SerializeField] private Changelog changelog;

    private Guid layerID = Guid.NewGuid();
    private readonly Level level = new Level();

    public void SetTiles(Vector2Int[] positions, TileData[] tiles, string description) =>
        changelog.SendChange(new TileChangeInfo(description, layerID, positions, positions.Select(Level.GetTile).ToArray(), tiles));

    public Level Level => level;
    
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
                   Level.SetTile(tileChangeInfo.positions[i], tileChangeInfo.newTiles[i], layerID);
                }

                break;
        }
    }
    
    public void ClearAllTiles() => changelog.SendChange(level.ClearAllTilesChangeInfo);
    
    public void SetLevelData(LevelData levelData, GameTilePalette palette)
    {
        ClearAllTiles();
        
        changelog.SendChange(Level.SetLevelData(levelData, palette));
        changelog.ClearChangelog();
    }
}
