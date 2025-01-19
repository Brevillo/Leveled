using System;
using System.Linq;
using TMPro;
using UnityEngine;
using System.IO;

public class SaveDataManager : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameTilePalette palette;
    [SerializeField] private TMP_InputField folderPath;
    [SerializeField] private GameStateManager gameStateManager;

    private DataFolder<LevelData> saveFolder;

    private const string LevelFolder = "Levels";
    private const string LastFolderPathKey = "LastFolderPath";
    
    private void Awake()
    {
        saveFolder = new(".level", LevelFolder, () => new LevelData());
        
        folderPath.text = PlayerPrefs.GetString(LastFolderPathKey, "");
    }

    private void Start()
    {
        gameStateManager.GameState = GameState.Editing;
        editorState.ActiveTile = palette.Tiles.FirstOrDefault(tile => tile.TileBase != null);
        editorState.ActiveTool = ToolType.Brush;
        
        Load();
    }

    public void Save()
    {
        var invalidPathChars = Path.GetInvalidPathChars();
        if (folderPath.text.Any(c => invalidPathChars.Contains(c)))
        {
            Debug.LogError("Invalid Path Name!");
            return;
        }

        saveFolder.Save(editorState.GetLevelData(palette), folderPath.text); 
        PlayerPrefs.SetString(LastFolderPathKey, folderPath.text);
    }

    public void Load()
    {
        string loadingName = Path.GetFileNameWithoutExtension(folderPath.text);

        var levelData = saveFolder.GetDataFromFile(saveFolder.GetAllFiles()
            .FirstOrDefault(file => Path.GetFileNameWithoutExtension(file.FullName) == loadingName));

        if (levelData == null) return;
        
        editorState.SetLevelData(levelData, palette);
        PlayerPrefs.SetString(LastFolderPathKey, folderPath.text);
    }
    
    public void OpenLevelFolder()
    {
        OpenInFileBrowser($"{Application.persistentDataPath}/{LevelFolder}");
    }

    public void New()
    {
        editorState.ClearAllTiles();
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
