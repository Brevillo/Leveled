using System;
using System.Linq;
using TMPro;
using UnityEngine;
using System.IO;

public class TileEditorManager : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameTilePalette palette;
    [SerializeField] private TMP_InputField folderPath;

    private DataFolder<LevelData> saveFolder;

    private const string LevelFolder = "Levels";
    private const string LastFolderPathKey = "LastFolderPath";
    
    private void Awake()
    {
        saveFolder = new(".level", LevelFolder, data => data.name, () => new LevelData());
        
        editorState.GameState = GameState.Editing;
        editorState.ActiveTile = palette.Tiles.FirstOrDefault(tile => tile.TileBase != null);
        editorState.ActiveTool = ToolType.Brush;
        
        editorState.ResetTiles();

        folderPath.text = PlayerPrefs.GetString(LastFolderPathKey, "");
    }

    private void Start()
    {
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

        string path = $"{Application.persistentDataPath}/Levels/{folderPath.text}";
        
        var saveData = new LevelData
        {
            name = Path.GetFileNameWithoutExtension(path), 
            positions = editorState.Tiles.Keys.ToArray(),
            ids = editorState.Tiles.Values.ToArray(),
        };
        
        saveFolder.Save(saveData); 
        
        PlayerPrefs.SetString(LastFolderPathKey, folderPath.text);
    }

    public void Load()
    {
        string loadingName = Path.GetFileNameWithoutExtension(folderPath.text);

        var save = saveFolder.GetAllSavedData()
            .FirstOrDefault(save => save.name == loadingName);

        if (save == null) return;
        
        PlayerPrefs.SetString(LastFolderPathKey, folderPath.text);
        
        editorState.LoadLevel(save);
    }

    public void OpenLevelFolder()
    {
        OpenInFileBrowser($"{Application.persistentDataPath}/{LevelFolder}");
    }
    
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
}
