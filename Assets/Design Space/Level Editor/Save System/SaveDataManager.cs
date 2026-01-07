using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using System.IO;
using OliverBeebe.UnityUtilities.Runtime.Settings;
using SFB;
using UnityEngine.UI;

public enum AutosaveOptions
{
    Never = 0,
    EveryChange = 1,
    Periodic = 2,
}

public class SaveDataManager : MonoBehaviour
{
    [SerializeField] private LeveledSaveSystem saveSystem;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameTilePalette palette;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private IntSetting autosaveSetting;
    [Header("Save UI")] 
    [SerializeField] private ConfirmationMenu confirmationMenu;
    [SerializeField] private GameObject dirtyFlag;
    [Header("Level Select UI")]
    [SerializeField] private Changelog changelog;
    [SerializeField] private TMP_InputField levelName;
    [SerializeField] private LevelSelectOption levelSelectOptionPrefab;
    [SerializeField] private Transform levelSelectionOptionsParent;
    [SerializeField] private CameraMovement cameraMovement;
    
    
    private float autosaveTimer;

    private readonly List<LevelSelectOption> levelSelectOptionInstances = new();
    
    private AutosaveOptions AutosaveOption => (AutosaveOptions)autosaveSetting.Value;
    
    private void OnEnable()
    {
        changelog.ChangeEvent += OnChangeEvent;
        changelog.LogUpdated += OnChangelogUpdated;
        saveSystem.FolderUpdated += OnFolderUpdated;
    }

    private void OnDisable()
    {
        changelog.ChangeEvent -= OnChangeEvent;
        changelog.LogUpdated -= OnChangelogUpdated;
        saveSystem.FolderUpdated -= OnFolderUpdated;
    }

    private void OnChangeEvent(ChangeInfo changeInfo)
    {
        if (AutosaveOption == AutosaveOptions.EveryChange)
        {
            TrySaveLevel();
        }
    }

    private void OnChangelogUpdated(Changelog.LogUpdateType logUpdateType)
    {
        dirtyFlag.SetActive(changelog.ActiveLevelDirty);
    }

    private void OnFolderUpdated()
    {
        RefreshLevels();
        LoadLevel(saveSystem.RecentSaves.FirstOrDefault());
    }

    private void Start()
    {
        gameStateManager.SetEditorStateAndNotify(EditorState.Editing);
        OnFolderUpdated();
    }

    private void Update()
    {
        autosaveTimer += Time.deltaTime;

        if (AutosaveOption == AutosaveOptions.Periodic && autosaveTimer > 5f)
        {
            TrySaveLevel();
            autosaveTimer = 0;
        }
    }

    public void RefreshLevels()
    {
        foreach (var option in levelSelectOptionInstances)
        {
            Destroy(option.gameObject);
        }

        levelSelectOptionInstances.Clear();

        foreach (var levelName in saveSystem.RecentSaves)
        {
            var option = Instantiate(levelSelectOptionPrefab, levelSelectionOptionsParent);
            
            option.Initialize(levelName);
            
            option.Select.onClick.AddListener(() => RequestSaveLevel(() => LoadLevel(levelName)));
            option.Delete.onClick.AddListener(() => DeleteLevel(levelName, option));
            option.ViewFile.onClick.AddListener(() => ViewFile(levelName));

            option.Selected = saveSystem.ActiveSaveName == levelName;
            
            levelSelectOptionInstances.Add(option);
        }
    }

    private void ViewFile(string levelName)
    {
        Process.Start(saveSystem.FolderPath);
    }

    private void DeleteLevel(string levelName, LevelSelectOption option)
    {
        confirmationMenu.OpenDestructiveConfirmMenu(
            $"Delete {levelName}?\n<size=50%>This action cannot be undone.</size>",
            "Delete",
            "Cancel",
            () =>
            {
                saveSystem.Delete(levelName);
                levelSelectOptionInstances.Remove(option);
                Destroy(option.gameObject);

                var levels = saveSystem.RecentSaves;
                if (levels.Length > 0)
                {
                    LoadLevel(levels[0]);
                }
                else
                {
                    NewLevel();
                }
            });
    }
    
    public void TrySaveLevel()
    {
        if (saveSystem.ActiveSaveName != levelName.text)
        {
            string oldName = saveSystem.ActiveSaveName;
            string newName = levelName.text;
            
            confirmationMenu.OpenDoubleOptionMenu(
                $"Save {newName} as new level?",
                "Save as new level.",
                $"Rename {oldName} to {newName}", 
                "Cancel",
                SaveAsNewLevel,
                () =>
                {
                    saveSystem.Delete(oldName);
                    SaveAsNewLevel();
                });
        }
        else
        {
            SaveLevel();
        }
    }

    private void SaveAsNewLevel()
    {
        saveSystem.CreateNewSave(levelName.text);
        SaveLevel();
    }
    
    private void SaveLevel()
    {
        saveSystem.SaveActiveSave(editorState.Level.GetData());
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
        
        confirmationMenu.OpenOptionalConfirmMenu(
            $"Save {saveSystem.ActiveSaveName}?", 
            "Save",
            "Don't Save",
            "Cancel",
            () =>
            {
                TrySaveLevel();
                action?.Invoke();
            }, 
            action);
    }
    
    private void LoadLevel(string name)
    {
        if (!saveSystem.IsValidName(name)) return;
        
        // Data, UX
        editorState.SetLevelData(saveSystem.Load(name), palette);
        cameraMovement.CenterCameraOnLevel();

        // UI
        levelName.SetTextWithoutNotify(name);
        RefreshLevels();
    }
    
    public void ChooseLevelFolder()
    {
        string path = 
            StandaloneFileBrowser.OpenFolderPanel("Choose Folder to Load Levels From", saveSystem.FolderPath, false)
            .FirstOrDefault();

        if (path == default) return;

        saveSystem.FolderPath = path;
        
        RefreshLevels();
    }

    public void NewLevel()
    {
        RequestSaveLevel(() =>
        {
            string path = StandaloneFileBrowser.SaveFilePanel("Create New Level", saveSystem.FolderPath, "New Level", "");

            if (path == "") return;
            
            editorState.ClearAllTiles();
            changelog.ClearChangelog();
            
            levelName.SetTextWithoutNotify(Path.GetFileNameWithoutExtension(path));
            
            SaveAsNewLevel();
        });
    }
}
