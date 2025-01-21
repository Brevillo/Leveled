using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = "Leveled/GameTile Editor State")]
public class TileEditorState : GameService
{
    [SerializeField] private Changelog changelog;
    
    #region State
    
    private GameTile activeTile = null;
    private ToolType activeTool = ToolType.Brush;
    
    private Dictionary<Vector3Int, TileData> tiles = new();
    
    private string changeInfoBundleDescription;
    private List<ChangeInfo> changeInfoBundle;
    
    #endregion

    protected override void Initialize()
    {
        changelog.ChangeEvent += OnChangelogChangeEvent;
    }

    private void OnChangelogChangeEvent(ChangeInfo changeInfo)
    {
        SendEditorChange(changeInfo, false);
    }

    #region Changes
    
    public event Action<ChangeInfo> EditorChanged;

    public void StartChangeBundle(string description)
    {
        changeInfoBundleDescription = description;
        changeInfoBundle = new();
    }

    public void EndChangeBundle()
    {
        if (changeInfoBundle != null)
        {
            changelog.LogChange(new ChangeInfoBundle(changeInfoBundleDescription, changeInfoBundle.ToArray()));
        }

        changeInfoBundle = null;
        changeInfoBundleDescription = "";
    }
    
    private void SendEditorChange(ChangeInfo changeInfo, bool logChange = true)
    {
        if (changeInfoBundle != null)
        {
            changeInfoBundle.Add(changeInfo);
        }
        else if (logChange)
        {
            changelog.LogChange(changeInfo);
        }
        
        EditorChanged?.Invoke(changeInfo);
        
        switch (changeInfo)
        {
            case ChangeInfoBundle changeInfoBundle:

                foreach (var info in changeInfoBundle.changeInfos)
                {
                    SendEditorChange(info, false);
                }
                
                break;
            
            case PaletteChangeInfo paletteChangeInfo:
                activeTile = paletteChangeInfo.newTile;
                break;
            
            case ToolbarChangeInfo toolbarChangeInfo:
                activeTool = toolbarChangeInfo.newTool;
                break;
            
            case TileChangeInfo tileChangeInfo:

                if (tileChangeInfo.newTile.IsEmpty)
                {
                    tiles.Remove(tileChangeInfo.position);
                }
                else
                {
                    tiles[tileChangeInfo.position] = tileChangeInfo.newTile;
                }
                break;
            
            case MultiTileChangeInfo multiTileChangeInfo:

                for (int i = 0; i < multiTileChangeInfo.positions.Length; i++)
                {
                    var tile = multiTileChangeInfo.newTiles[i];
            
                    if (tile.IsEmpty)
                    {
                        tiles.Remove(multiTileChangeInfo.positions[i]);
                    }
                    else
                    {
                        tiles[multiTileChangeInfo.positions[i]] = tile;
                    }
                }
                
                break;
        }
    }
    
    #endregion
    
    #region State Modification/Access

    public bool PointerOverUI
    {
        get
        {
            int uiLayer = LayerMask.NameToLayer("UI");
            
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition,
            };
            var results = new List<RaycastResult>();
            
            EventSystem.current.RaycastAll(eventData, results);

            return results.Any(result => result.gameObject.layer == uiLayer);
        }
    }
    
    public string[] LinkingGroups => tiles.Values
        .GroupBy(tile => tile.linkingGroup)
        .Select(group => group.Key)
        .Where(group => group != string.Empty)
        .ToArray();

    public string GetLinkingGroup(GameObject gameObject)
    {
        var grid = gameObject.GetComponentInParent<Grid>();
        Vector3Int position = grid.WorldToCell(gameObject.transform.position);
        
        return tiles.TryGetValue(position, out var tile)
            ? tile.linkingGroup
            : string.Empty;
    }

    public TileData GetTile(Vector3Int position) => tiles.GetValueOrDefault(position);

    public void SetTile(Vector3Int position, TileData tile)
    {
        var current = GetTile(position);

        if (tile == current)
        {
            return;
        }
        
        SendEditorChange(new TileChangeInfo(position, current, tile));
    }

    public void SetTiles(Vector3Int[] positions, TileData[] tiles) =>
        SendEditorChange(new MultiTileChangeInfo(positions, positions.Select(GetTile).ToArray(), tiles));
    
    public GameTile ActiveTile
    {
        get => activeTile;
        set => SendEditorChange(new PaletteChangeInfo(activeTile, value), false);
    }

    public void SetActiveTool(string toolTypeName)
    {
        if (Enum.TryParse(typeof(ToolType), toolTypeName, true, out var toolType))
        {
            ActiveTool = (ToolType)toolType;
        }
    }
    
    public ToolType ActiveTool
    {
        get => activeTool;
        set => SendEditorChange(new ToolbarChangeInfo(activeTool, value), false);
    }
    
    #endregion
    
    #region Get/Set Level Data
    
    public LevelData GetLevelData() => new()
    {
        positions = tiles.Keys.ToArray(),
        gameTileIds = tiles.Values.Select(data => data.gameTile.ID).ToArray(),
        linkingGroups = tiles.Values.Select(data => data.linkingGroup).ToArray(),
    };

    public void ClearAllTiles()
    {
        // Clear Tiles
        var nullTiles = new TileData[tiles.Count];
        
        SetTiles(tiles.Keys.ToArray(), nullTiles);
        
        // Reset editor state
        changelog.ClearChangelog();
        tiles.Clear();
    }
    
    public void SetLevelData(LevelData levelData, GameTilePalette palette)
    {
        // Clear current level
        var nullTiles = new TileData[tiles.Count];
        
        SetTiles(tiles.Keys.ToArray(), nullTiles);
        
        // Fill new level
        SetTiles(levelData.positions, Enumerable.Range(0, levelData.positions.Length)
            .Select(i => new TileData(palette.GetTile(levelData.gameTileIds[i]), levelData.linkingGroups[i]))
            .ToArray());
        
        // Clear changelog
        changelog.ClearChangelog();
    }
    
    #endregion
}
