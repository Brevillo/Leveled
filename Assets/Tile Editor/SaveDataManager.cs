using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using System.IO;
using SFB;
using UnityEngine.UI;

public class SaveDataManager : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameTilePalette palette;
    [SerializeField] private GameStateManager gameStateManager;
    [Header("Save UI")] 
    [SerializeField] private ConfirmationMenu confirmationMenu;
    [Header("Level Select UI")]
    [SerializeField] private Changelog changelog;
    [SerializeField] private TMP_InputField levelName;
    [SerializeField] private LevelSelectOption levelSelectOptionPrefab;
    [SerializeField] private Transform levelSelectionOptionsParent;
    [SerializeField] private CameraMovement cameraMovement;
    
    private DataFolder<LevelData> saveFolder;

    private string loadedLevelName;
    
    private const string LastFolderPathPrefKey = "LastFolderPath";
    private const string LastLevelNamePrefKey = "LastLevelName";
    private const string FileExtension = "level";

    private Dictionary<string, LevelSelectOption> levelSelectOptionInstances;
    
    private string LevelFolderPath
    {
        get => saveFolder?.directory.FullName ?? "";
        set
        {
            if (value == "") return;
            
            saveFolder = new($".{FileExtension}", value, () => new());
        }
    }
    
    private static string GetName(FileInfo file) => Path.GetFileNameWithoutExtension(file.FullName);
    
    private void Awake()
    {
        levelSelectOptionInstances = new();
        LevelFolderPath = PlayerPrefs.GetString(LastFolderPathPrefKey, "");
    }

    private void Start()
    {
        gameStateManager.GameState = GameState.Editing;
        editorState.ActiveTile = palette.Tiles.FirstOrDefault(tile => tile.TileBase != null);
        editorState.ActiveTool = ToolType.Brush;
        
        RefreshLevels();
        
        if (saveFolder != null)
        {
            var levels = saveFolder.GetAllFiles();
            string lastLevelName = PlayerPrefs.GetString(LastLevelNamePrefKey, "");
            var lastLevelFile = levels.FirstOrDefault(file => GetName(file) == lastLevelName);

            LoadLevel(lastLevelFile ?? levels.FirstOrDefault());
        }
    }

    public void RefreshLevels()
    {
        if (saveFolder == null)
        {
            return;
        }
        
        foreach (var option in levelSelectOptionInstances.Values)
        {
            Destroy(option.gameObject);
        }

        levelSelectOptionInstances.Clear();

        foreach (var file in saveFolder.GetAllFiles())
        {
            var option = Instantiate(levelSelectOptionPrefab, levelSelectionOptionsParent);

            string levelName = GetName(file);
            
            option.Initialize(levelName);
            
            option.Select.onClick.AddListener(() => RequestSaveLevel(() => LoadLevel(file)));
            option.Delete.onClick.AddListener(() => DeleteLevelOption(file, option));

            option.Selected = loadedLevelName == levelName;
            
            levelSelectOptionInstances.Add(levelName, option);
        }
    }

    private void DeleteLevelOption(FileInfo file, LevelSelectOption option)
    {
        confirmationMenu.OpenDestructiveConfirmMenu("Delete Level?", "Delete", "Cancel",
            () =>
            {
                file.Delete();
                levelSelectOptionInstances.Remove(GetName(file));
                Destroy(option.gameObject);
            });
    }
    
    public void SaveLevel()
    {
        saveFolder.Delete(loadedLevelName);
        saveFolder.Save(editorState.GetLevelData(), levelName.text);

        loadedLevelName = levelName.text;
        
        changelog.NotifySaved();
        
        RefreshLevels();
    }

    private void RequestSaveLevel(Action action)
    {
        if (!changelog.ActiveLevelDirty)
        {
            action?.Invoke();
            return;
        }
        
        confirmationMenu.OpenOptionalConfirmMenu("Save Level?", "Save", "Don't Save", "Cancel",
            () =>
            {
                SaveLevel();
                action?.Invoke();
            }, 
            action);
    }
    
    private void LoadLevel(FileInfo file)
    {
        if (file == null) return;

        string loadingLevelName = GetName(file);
        
        PlayerPrefs.SetString(LastLevelNamePrefKey, loadingLevelName);
        
        var levelData = saveFolder.GetDataFromFile(file);
    
        if (levelData == null) return;

        TryLoadAs(new LevelData_1(), 
            data => data.positions,
            data => data.ids,
            _ => new string[levelData.positions.Length]);
        
        TryLoadAs(new LevelData_2(),
            data => data.positions,
            data => data.paletteIndices,
            data => data.linkingGroups);
        
        if (levelData.gameTileIds == null
            || levelData.positions == null
            || levelData.linkingGroups == null) return;

        if (loadedLevelName != null
            && levelSelectOptionInstances.TryGetValue(loadedLevelName, out var levelSelectOption))
        {
            levelSelectOption.Selected = false;
        }
        
        levelSelectOptionInstances[loadingLevelName].Selected = true;
        
        levelName.text = loadedLevelName = loadingLevelName;
        
        editorState.SetLevelData(levelData, palette);
        cameraMovement.CenterCameraOnLevel();

        void TryLoadAs<T>(T data,
            Func<T, Vector3Int[]> positionGetter,
            Func<T, int[]> idsGetter,
            Func<T, string[]> linkingGroupsGetter)
        {
            JsonUtility.FromJsonOverwrite(File.ReadAllText(file.FullName), data);

            levelData.positions ??= positionGetter.Invoke(data);
            levelData.gameTileIds ??= idsGetter.Invoke(data);
            levelData.linkingGroups ??= linkingGroupsGetter.Invoke(data);
        }
    }
    
    public void ChooseLevelFolder()
    {
        string path = StandaloneFileBrowser.OpenFolderPanel("Choose Folder to Load Levels From", LevelFolderPath, false)
            .FirstOrDefault();

        if (path == default) return;

        PlayerPrefs.SetString(LastFolderPathPrefKey, path);
        LevelFolderPath = path;
        
        RefreshLevels();
    }

    public void NewLevel()
    {
        RequestSaveLevel(() =>
        {
            string path =
                StandaloneFileBrowser.SaveFilePanel("Create New Level", LevelFolderPath, "New Level", FileExtension);

            if (path == "") return;
            
            levelName.text = loadedLevelName = Path.GetFileNameWithoutExtension(path);
            
            editorState.ClearAllTiles();
            
            SaveLevel();
            RefreshLevels();
        });
        
    }
}
