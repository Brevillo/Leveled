using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

public interface ISaveSystem<TData>
{
    public string ActiveSaveName { get; }
    
    public TData ActiveSaveData { get; }
    
    public bool FolderChosen { get; }
    
    public string FolderPath { get; set; }

    public string[] AllSaveNames { get; }

    public bool IsValidName(string name);

    public bool TryLoad(string name, out TData data);
    
    public TData Load(string name);

    public void SaveActiveSave(TData data);

    public void CreateNewSave(string name);

    public void Delete(string name);

    public event Action FolderUpdated;
}

public interface IPlayerPrefResettable
{
    public void ResetPlayerPrefs();
}

public abstract class SaveSystem<TData>
    : ScriptableObject, ISaveSystem<TData>, IPlayerPrefResettable
{
    [SerializeField] private string extension;
    [SerializeField] private string lastFolderPathPrefKey;
    
    private DirectoryInfo directory;
    
    private string activeSaveName;
    private TData activeSaveData;
    
    public string ActiveSaveName => activeSaveName;

    public TData ActiveSaveData => activeSaveData;

    public bool FolderChosen => Directory != null;

    private void UpdateDirectory(string name)
    {
        if (name == string.Empty || (directory != null && directory.FullName == name)) return;
        
        PlayerPrefs.SetString(lastFolderPathPrefKey, name);
        
        directory = new(name);
        directory.Create();
        FolderUpdated?.Invoke();
    }
    
    private DirectoryInfo Directory
    {
        get
        {
            UpdateDirectory(PlayerPrefs.GetString(lastFolderPathPrefKey));
            return directory;
        }
    }

    public string FolderPath
    {
        get => FolderChosen ? Directory.FullName : string.Empty;
        set => UpdateDirectory(value);
    }

    public string[] AllSaveNames => FolderChosen 
        ? Directory.GetFiles()
            .Where(file => file != null && file.Extension == extension)
            .Select(file => Path.GetFileNameWithoutExtension(file.FullName))
            .ToArray()
        : Array.Empty<string>();

    public bool IsValidName(string name) => FolderChosen && File.Exists(FilePath(name));
    
    public bool TryLoad(string name, out TData data)
    {
        bool valid = IsValidName(name);

        data = valid ? Load(name) : default;

        return valid;
    }

    public TData Load(string name)
    {
        if (!FolderChosen) return default;
        
        if (!IsValidName(name))
        {
            throw new Exception("Tried to load invalid save name!");
        }
        
        activeSaveName = name;
        activeSaveData = GetSaveData(activeSaveName);
        
        OnFileAccessed(ref activeSaveData);
        
        return activeSaveData;
    }
    
    public void SaveActiveSave(TData data)
    {
        if (!FolderChosen) return;
        
        OnFileAccessed(ref data);

        activeSaveData = data;
        string json = JsonConvert.SerializeObject(data);
        
        File.WriteAllText(FilePath(activeSaveName), json);
    }

    public void CreateNewSave(string name)
    {
        if (!FolderChosen) return;

        activeSaveName = name;
    }
    
    public void Delete(string name)
    {
        if (!FolderChosen) return;
        
        File.Delete(FilePath(name));

        if (activeSaveName == name)
        {
            activeSaveName = string.Empty;
            activeSaveData = default;
        }
    }

    public string FilePath(string name) => $"{Directory.FullName}/{name}{extension}";
    
    public event Action FolderUpdated;

    #region Internals
    
    protected virtual void OnFileAccessed(ref TData data) { }

    protected TData GetSaveData(string name)
    {
        if (!FolderChosen) return default;
        
        string path = FilePath(name);

        if (!File.Exists(path)) return default;
        
        string json = File.ReadAllText(FilePath(name));
        return JsonConvert.DeserializeObject<TData>(json);
    }
    
    void IPlayerPrefResettable.ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(lastFolderPathPrefKey);
    }
    
    #endregion
}

#if UNITY_EDITOR

[CustomEditor(typeof(SaveSystem<>), true)]
public class SaveSystemEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        InspectorElement.FillDefaultInspector(root, serializedObject, this);
        
        root.Add(new Button(((IPlayerPrefResettable)target).ResetPlayerPrefs)
        {
            text = "Reset Folder Pref Data",
        });

        return root;
    }
}

#endif