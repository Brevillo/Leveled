using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public interface ISaveSystem<TData>
{
    public string ActiveSaveName { get; }
    
    public TData ActiveSaveData { get; }
    
    public string FolderPath { get; set; }

    public string[] AllSaveNames { get; }

    public bool IsValidName(string name);

    public bool TryLoad(string name, out TData data);
    
    public TData Load(string name);

    public void SaveActiveSave(TData data);

    public void CreateNewSave(string name);

    public void Delete(string name);
}

public abstract class SaveSystem<TData> : ISaveSystem<TData>
{
    private readonly string extension;
    private readonly string lastFolderPathPrefKey;
    
    private DirectoryInfo directory;
    
    private string activeSaveName;
    private TData activeSaveData;
    
    protected SaveSystem(string extension, string lastFolderPathPrefKey)
    {
        this.extension = extension;
        this.lastFolderPathPrefKey = lastFolderPathPrefKey;

        string folderPath = PlayerPrefs.GetString(lastFolderPathPrefKey, string.Empty);
        if (folderPath != string.Empty)
        {
            FolderPath = folderPath;
        }
    }

    public string ActiveSaveName => activeSaveName;

    public TData ActiveSaveData => activeSaveData;
    
    public string FolderPath
    {
        get => directory?.FullName ?? string.Empty;
        set
        {
            directory = new DirectoryInfo(value);
            PlayerPrefs.SetString(lastFolderPathPrefKey, value);
            
            if (!directory.Exists)
            {
                directory.Create();
            }
        }
    }

    public string[] AllSaveNames => directory?.GetFiles()
        .Where(file => file != null && file.Extension == extension)
        .Select(file => Path.GetFileNameWithoutExtension(file.FullName))
        .ToArray() ?? Array.Empty<string>();

    public bool IsValidName(string name) => directory != null && File.Exists(FilePath(name));
    
    public bool TryLoad(string name, out TData data)
    {
        bool valid = IsValidName(name);

        data = valid ? Load(name) : default;

        return valid;
    }

    public TData Load(string name)
    {
        if (directory == null) return default;
        
        if (!IsValidName(name))
        {
            throw new Exception("Tried to load invalid save name!");
        }
        
        activeSaveName = name;
        activeSaveData = GetSaveData(activeSaveName);
        
        activeSaveData = OnFileAccessed(activeSaveData);
        
        return activeSaveData;
    }
    
    public void SaveActiveSave(TData data)
    {
        if (directory == null) return;
        
        OnFileAccessed(data);

        activeSaveData = data;
        string json = JsonConvert.SerializeObject(data);
        
        File.WriteAllText(FilePath(activeSaveName), json);
    }

    public void CreateNewSave(string name)
    {
        if (directory == null) return;

        activeSaveName = name;
    }
    
    public void Delete(string name)
    {
        if (directory == null) return;
        
        File.Delete(FilePath(name));

        if (activeSaveName == name)
        {
            activeSaveName = string.Empty;
            activeSaveData = default;
        }
    }

    protected virtual TData OnFileAccessed(TData data) => data;

    protected TData GetSaveData(string name)
    {
        if (directory == null) return default;
        
        string path = FilePath(name);

        if (!File.Exists(path)) return default;
        
        string json = File.ReadAllText(FilePath(name));
        return JsonConvert.DeserializeObject<TData>(json);
    }
    
    private string FilePath(string name) => $"{directory.FullName}/{name}{extension}";
}

public class LeveledSaveSystem : SaveSystem<LevelData>
{
    public LeveledSaveSystem(string extension, string lastFolderPathPrefKey) 
        : base(extension, lastFolderPathPrefKey)
    { }

    protected override LevelData OnFileAccessed(LevelData data)
    {
        // Update last time accessed
        data.lastTimeAccessed = DateTime.Now;
        
        // Compute grid size
        Vector2Int min = Vector2Int.one * int.MaxValue;
        Vector2Int max = Vector2Int.one * int.MinValue;
        
        foreach (var position in data.positions)
        {
            min = Vector2Int.Min(min, position);
            max = Vector2Int.Max(max, position);
        }

        data.gridSize = max - min;

        return data;
    }

    public string[] RecentSaves => AllSaveNames
        .OrderByDescending(save => GetSaveData(save).lastTimeAccessed)
        .ToArray();
}