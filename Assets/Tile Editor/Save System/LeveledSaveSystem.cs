using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Editor Systems/Save System")]
public class LeveledSaveSystem : SaveSystem<LevelData>
{
    protected override void OnFileAccessed(ref LevelData data)
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
    }

    public string[] RecentSaves => AllSaveNames
        .OrderByDescending(save => GetSaveData(save).lastTimeAccessed)
        .ToArray();
}