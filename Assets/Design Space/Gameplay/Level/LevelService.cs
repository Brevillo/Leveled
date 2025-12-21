using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Level Service")]
public class LevelService : ScriptableObject
{
    private LevelParent levelParent;

    private HashSet<LevelResettable> levelResettables;
    
    public void RegisterLevel(LevelParent levelParent)
    {
        this.levelParent = levelParent;

        levelResettables = new();
    }
    
    public void ResetLevel()
    {
        if (levelParent == null) return;

        foreach (var resettable in levelResettables)
        {
            resettable.DoReset();
        }
    }

    public void RegisterResettable(LevelResettable levelResettable)
    {
        levelResettables.Add(levelResettable);
    }

    public void DeregisterResettable(LevelResettable levelResettable)
    {
        levelResettables.Remove(levelResettable);
    }
}