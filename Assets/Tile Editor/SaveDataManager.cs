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
    [SerializeField] private TMP_InputField levelName;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private LevelSelectOption levelSelectOptionPrefab;
    [SerializeField] private Transform levelSelectionOptionsParent;
    [SerializeField] private CameraMovement cameraMovement;
    
    private DataFolder<LevelData> saveFolder;

    private string loadedLevelName;
    
    private const string LastFolderPathPrefKey = "LastFolderPath";
    private const string LastLevelNamePrefKey = "LastLevelName";
    private const string FileExtension = "level";

    private List<LevelSelectOption> levelSelectOptionInstances;
    
    private string LevelFolderPath
    {
        get => saveFolder?.directory.FullName ?? "";
        set
        {
            if (value == "") return;
            
            saveFolder = new($".{FileExtension}", value, () => new LevelData());
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
        
        foreach (var option in levelSelectOptionInstances)
        {
            Destroy(option.gameObject);
        }

        levelSelectOptionInstances.Clear();

        foreach (var file in saveFolder.GetAllFiles())
        {
            var option = Instantiate(levelSelectOptionPrefab, levelSelectionOptionsParent);

            option.Initialize(GetName(file));
            
            option.Select.onClick.AddListener(() => LoadLevel(file));
            option.Delete.onClick.AddListener(() => DeleteLevelOption(file, option));
            
            option.UpdateSelected(loadedLevelName);
            
            levelSelectOptionInstances.Add(option);
        }
    }

    private void DeleteLevelOption(FileInfo file, LevelSelectOption option)
    {
        file.Delete();
        levelSelectOptionInstances.Remove(option);
        Destroy(option.gameObject);
    }
    
    public void SaveLevel()
    {
        saveFolder.Delete(loadedLevelName);
        saveFolder.Save(editorState.GetLevelData(palette), levelName.text);

        loadedLevelName = levelName.text;
        
        RefreshLevels();
    }

    private void LoadLevel(FileInfo file)
    {
        if (file == null) return;
        
        PlayerPrefs.SetString(LastLevelNamePrefKey, GetName(file));
        
        var levelData = saveFolder.GetDataFromFile(file);
    
        if (levelData == null) return;

        if (levelData.paletteIndices == null)
        {
            var levelData_1 = new LevelData_1();
            JsonUtility.FromJsonOverwrite(File.ReadAllText(file.FullName), levelData_1);

            levelData.paletteIndices = levelData_1.ids;
            levelData.linkingGroups = new string[levelData.positions.Length];
        }

        if (levelData.paletteIndices == null) return;

        levelName.text = loadedLevelName = GetName(file);
        
        editorState.SetLevelData(levelData, palette);
        cameraMovement.ResetCamera();
        
        foreach (var option in levelSelectOptionInstances)
        {
            option.UpdateSelected(GetName(file));
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
        print(LevelFolderPath);

        string path =
            StandaloneFileBrowser.SaveFilePanel("Create New Level", LevelFolderPath, "New Level", FileExtension);

        if (path == "") return;
        
        levelName.text = loadedLevelName = Path.GetFileNameWithoutExtension(path);
        
        editorState.ClearAllTiles();
        
        SaveLevel();
        RefreshLevels();
    }
    
    #region Open File Browser
    
    public static void OpenInMacFileBrowser(string path)
    {
        bool openInsidesOfFolder = false;

        // try mac
        string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

        if (Directory.Exists(macPath)) // if path requested is a folder, automatically open insides of that folder
        {
            openInsidesOfFolder = true;
        }

        //Debug.Log("macPath: " + macPath);
        //Debug.Log("openInsidesOfFolder: " + openInsidesOfFolder);

        if (!macPath.StartsWith("\""))
        {
            macPath = "\"" + macPath;
        }
        if (!macPath.EndsWith("\""))
        {
            macPath = macPath + "\"";
        }
        string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;
        //Debug.Log("arguments: " + arguments);
        try
        {
            System.Diagnostics.Process.Start("open", arguments);
        }
        catch(System.ComponentModel.Win32Exception e)
        {
            // tried to open mac finder in windows
            // just silently skip error
            // we currently have no platform define for the current OS we are in, so we resort to this
            e.HelpLink = ""; // do anything with this variable to silence warning about not using it
        }
    }

    public static void OpenInWinFileBrowser(string path)
    {
        bool openInsidesOfFolder = false;

        // try windows
        string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

        if (Directory.Exists(winPath)) // if path requested is a folder, automatically open insides of that folder
        {
            openInsidesOfFolder = true;
        }
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
        }
        catch(System.ComponentModel.Win32Exception e)
        {
            // tried to open win explorer in mac
            // just silently skip error
            // we currently have no platform define for the current OS we are in, so we resort to this
            e.HelpLink = ""; // do anything with this variable to silence warning about not using it
        }
    }

    public static void OpenInFileBrowser(string path)
    {
        OpenInWinFileBrowser(path);
        OpenInMacFileBrowser(path);
    }
    
    #endregion
}
