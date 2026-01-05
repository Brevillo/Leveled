using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = ProjectConstants.ChangelogFolder + "Change Log")]
public class Changelog : ScriptableObject
{
    [SerializeField] private List<ChangeloggedValue> changeloggedValues;
    
    private readonly Stack<ChangeInfo> undoLog = new();
    private readonly Stack<ChangeInfo> redoLog = new();

    private int undosForCleanFile = 0;

    private string changeInfoBundleDescription;
    private List<ChangeInfo> changeInfoBundle;

    public Stack<ChangeInfo> UndoLog => undoLog;
    public Stack<ChangeInfo> RedoLog => redoLog;

    public enum LogUpdateType
    {
        Saved,
        NewChange,
        Undo,
        Redo,
        Cleared,
    }

    public List<ChangeloggedValue> ChangeloggedValues => changeloggedValues;
    
    public bool ActiveLevelDirty => undoLog.Count != undosForCleanFile;
    
    public event Action<LogUpdateType> LogUpdated;
    
    public event Action<ChangeInfo> StateUpdated;
    public event Action<ChangeInfo> ChangeEvent;

    public void Initialize()
    {
        foreach (var value in changeloggedValues)
        {
            value.Initialize(this);
        }
    }

    public void Cleanup()
    {
        foreach (var value in changeloggedValues)
        {
            value.Cleanup();
        }
    }
    
    public void NotifySaved()
    {
        undosForCleanFile = undoLog.Count;
        LogUpdated?.Invoke(LogUpdateType.Saved);
    }

    public void StartChangeBundle(string description)
    {
        changeInfoBundleDescription = description;
        changeInfoBundle = new();
    }

    public void EndChangeBundle()
    {
        if (changeInfoBundle == null) return;
        
        var changeBundle = new ChangeInfoBundle(changeInfoBundleDescription, changeInfoBundle.ToArray());

        changeInfoBundle = null;
        changeInfoBundleDescription = "";
        
        SendChange(changeBundle);
    }
    
    public void SendChange(ChangeInfo changeInfo, bool log = true)
    {
        StateUpdated?.Invoke(changeInfo);
        ChangeEvent?.Invoke(changeInfo);

        if (changeInfo is ChangeInfoBundle bundle)
        {
            foreach (var info in bundle.changeInfos)
            {
                SendChange(info, false);
            }
        }
        
        if (changeInfoBundle != null)
        {
            changeInfoBundle.Add(changeInfo);
        }
        else if (log)
        {
            undoLog.Push(changeInfo);
            redoLog.Clear();
            LogUpdated?.Invoke(LogUpdateType.NewChange);
        }
    }

    public void Undo(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Undo();
        }
    }
    
    public void Undo()
    {
        if (!undoLog.TryPop(out var change)) return;
        
        var undoChange = change.Reverted;
        
        SendChange(undoChange, false);
        redoLog.Push(undoChange);
        LogUpdated?.Invoke(LogUpdateType.Undo);
    }

    public void Redo(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Redo();
        }
    }

    public void Redo()
    {
        if (!redoLog.TryPop(out var change)) return;
        
        var redoChange = change.Reverted;
        
        SendChange(redoChange, false);
        undoLog.Push(redoChange);
        LogUpdated?.Invoke(LogUpdateType.Redo);
    }

    public void ClearChangelog()
    {
        undosForCleanFile = 0;
        undoLog.Clear();
        redoLog.Clear();
        LogUpdated?.Invoke(LogUpdateType.Cleared);
    }
}
