using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Tile Editor State")]
public class TileEditorState : GameService
{
    [SerializeField] private Changelog changelog;

    [Header("Defaults")]
    [SerializeField] private GameTile defaultPrimaryTile;
    [SerializeField] private GameTile defaultSecondaryTile;
    [SerializeField] private ToolbarAction defaultTool;
    [SerializeField] private bool defaultShowPlayerPositionRecording;
    [SerializeField] private bool defaultShowLinkingGroups;

    #region State

    private GameTile primaryTile;
    private GameTile secondaryTile;
    private ToolbarAction activeTool;

    private bool showLinkingGroups = false;
    private bool showPlayerPositionRecording = false;
    
    private Dictionary<Vector2Int, TileData> tiles = new();

    private string changeInfoBundleDescription;
    private List<ChangeInfo> changeInfoBundle;

    #endregion

    protected override void Initialize()
    {
        changelog.ChangeEvent += OnChangeEvent;
    }

    protected override void Start()
    {
        PrimaryTile = defaultPrimaryTile;
        SecondaryTile = defaultSecondaryTile;
        ActiveTool = defaultTool;
        ShowPlayerPositionRecording = defaultShowPlayerPositionRecording;
        ShowLinkingGroups = defaultShowLinkingGroups;
    }

    protected override void InstanceDestroyed()
    {
        changelog.ChangeEvent -= OnChangeEvent;
    }

    private void OnChangeEvent(ChangeInfo changeInfo)
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

    public void SendEditorChange(ChangeInfo changeInfo, bool logChange = true)
    {
        if (changeInfoBundle != null)
        {
            changeInfoBundle.Add(changeInfo);
        }
        else if (logChange)
        {
            changelog.LogChange(changeInfo);
        }

        OnEditorChanged(changeInfo);

        EditorChanged?.Invoke(changeInfo);
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case ChangeInfoBundle changeInfoBundle:

                foreach (var info in changeInfoBundle.changeInfos)
                {
                    SendEditorChange(info, false);
                }

                break;

            case PaletteChangeInfo paletteChangeInfo:

                switch (paletteChangeInfo.type)
                {
                    case PaletteChangeInfo.Type.Primary:
                        primaryTile = paletteChangeInfo.newValue;
                        break;
                    
                    case PaletteChangeInfo.Type.Secondary:
                        secondaryTile = paletteChangeInfo.newValue;
                        break;
                }

                break;

            case ToolbarChangeInfo toolbarChangeInfo:
                activeTool = toolbarChangeInfo.newValue;
                break;

            case TileChangeInfo tileChangeInfo:

                for (int i = 0; i < tileChangeInfo.positions.Length; i++)
                {
                    var tile = tileChangeInfo.newTiles[i];

                    if (tile.IsEmpty)
                    {
                        tiles.Remove(tileChangeInfo.positions[i]);
                    }
                    else
                    {
                        tiles[tileChangeInfo.positions[i]] = tile;
                    }
                }

                break;

            case ShowLinkingGroupsChangeInfo showLinkingGroupsChangeInfo:

                showLinkingGroups = showLinkingGroupsChangeInfo.newValue;

                break;
            
            case ShowPlayerPositionRecordingChangeInfo showPlayerPositionRecordingChangeInfo:

                showPlayerPositionRecording = showPlayerPositionRecordingChangeInfo.newValue;
                
                break;
        }
    }

#endregion
    
    #region State Modification/Access

    public bool PointerOverUI
    {
        get
        {
            Vector2 mousePosition = Input.mousePosition;

            if (mousePosition.x < 0
                || mousePosition.x > Screen.width
                || mousePosition.y < 0
                || mousePosition.y > Screen.height)
            {
                return true;
            }
            
            int uiLayer = LayerMask.NameToLayer("UI");
            
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = mousePosition,
            };
            var results = new List<RaycastResult>();
            
            EventSystem.current.RaycastAll(eventData, results);

            return results.Any(result => result.gameObject.layer == uiLayer);
        }
    }

    public (Vector2Int position, TileData tile)[] LinkedTiles => tiles
        .Where(kv => kv.Value.Linkable)
        .Select(kv => (kv.Key, kv.Value))
        .ToArray();
    
    public string[] LinkingGroups => tiles.Values
        .GroupBy(tile => tile.linkingGroup)
        .Select(group => group.Key)
        .Where(group => group != string.Empty)
        .ToArray();

    public string GetLinkingGroup(GameObject gameObject)
    {
        if (gameObject == null) return string.Empty;
        
        var grid = gameObject.GetComponentInParent<Grid>();
        if (grid == null) return string.Empty;
        
        Vector2Int position = (Vector2Int)grid.WorldToCell(gameObject.transform.position);
        
        return tiles.TryGetValue(position, out var tile)
            ? tile.linkingGroup
            : string.Empty;
    }

    public TileData GetTile(Vector2Int position) => tiles.GetValueOrDefault(position);

    public void SetTiles(Vector2Int[] positions, TileData[] tiles, string description) =>
        SendEditorChange(new TileChangeInfo(positions, positions.Select(GetTile).ToArray(), tiles, description));

    public GameTile PrimaryTile
    {
        get => primaryTile;
        set => SendEditorChange(new PaletteChangeInfo(primaryTile, value, PaletteChangeInfo.Type.Primary), false);
    }

    public GameTile SecondaryTile
    {
        get => secondaryTile;
        set => SendEditorChange(new PaletteChangeInfo(secondaryTile, value, PaletteChangeInfo.Type.Secondary), false);
    }

    public ToolbarAction ActiveTool
    {
        get => activeTool;
        set => SendEditorChange(new ToolbarChangeInfo(activeTool, value), false);
    }

    public bool ShowLinkingGroups
    {
        get => showLinkingGroups;
        set => SendEditorChange(new ShowLinkingGroupsChangeInfo(showLinkingGroups, value), false);
    }

    public bool ShowPlayerPositionRecording
    {
        get => showPlayerPositionRecording;
        set => SendEditorChange(new ShowPlayerPositionRecordingChangeInfo(showPlayerPositionRecording, value), false);
    }

    public Dictionary<Vector2Int, TileData> AllTiles => tiles;
    
    #endregion

    #region Helper Methods
    
    public void SwapSelectedTiles() => (PrimaryTile, SecondaryTile) = (SecondaryTile, PrimaryTile);

    public void ToggleShowLinkingGroups() => ShowLinkingGroups = !ShowLinkingGroups;

    public void ToggleShowPlayerPositionRecording() => ShowPlayerPositionRecording = !ShowPlayerPositionRecording;
    
    #endregion
    
    #region Get/Set Level Data
    
    public LevelData GetLevelData() => new()
    {
        positions = tiles.Keys.Select(vector => (SimpleVector2Int)vector).ToArray(),
        gameTileIds = tiles.Values.Select(data => data.gameTile.ID).ToArray(),
        linkingGroups = tiles.Values.Select(data => data.linkingGroup).ToArray(),
    };

    public void ClearAllTiles()
    {
        // Clear Tiles
        var nullTiles = new TileData[tiles.Count];
        
        SetTiles(tiles.Keys.ToArray(), nullTiles, "Cleared all tiles");
        
        // Reset editor state
        changelog.ClearChangelog();
        tiles.Clear();
    }
    
    public void SetLevelData(LevelData levelData, GameTilePalette palette)
    {
        // Clear current level
        var nullTiles = new TileData[tiles.Count];
        
        SetTiles(tiles.Keys.ToArray(), nullTiles, "Cleared all tiles");
        
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
