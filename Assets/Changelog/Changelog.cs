using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Leveled/Change Log")]
public class Changelog : GameService
{
    private readonly Stack<ChangeInfo> undoLog = new();
    private readonly Stack<ChangeInfo> redoLog = new();

    public Stack<ChangeInfo> UndoLog => undoLog;
    public Stack<ChangeInfo> RedoLog => redoLog;

    public bool ActiveLevelDirty => undoLog.Count != undosForCleanFile;

    private int undosForCleanFile = 0;
    
    public event Action LogUpdated;
    public event Action<ChangeInfo> ChangeEvent;

    public void NotifySaved()
    {
        undosForCleanFile = undoLog.Count;
        LogUpdated?.Invoke();
    }
    
    protected override void Initialize()
    {
        LogUpdated = null;
        ChangeEvent = null;
    }

    public void LogChange(ChangeInfo changeInfo)
    {
        undoLog.Push(changeInfo);
        redoLog.Clear();
        LogUpdated?.Invoke();
        ChangeEvent?.Invoke(changeInfo);
    }

    public void Undo()
    {
        if (!undoLog.TryPop(out var change)) return;
        
        var undoChange = change.Reverted;
        redoLog.Push(undoChange);
        LogUpdated?.Invoke();
        ChangeEvent?.Invoke(undoChange);
    }

    public void Redo()
    {
        if (!redoLog.TryPop(out var change)) return;
        
        var redoChange = change.Reverted;
        undoLog.Push(redoChange);
        LogUpdated?.Invoke();
        ChangeEvent?.Invoke(redoChange);
    }

    public void ClearChangelog()
    {
        undosForCleanFile = 0;
        undoLog.Clear();
        redoLog.Clear();
        LogUpdated?.Invoke();
    }
}
