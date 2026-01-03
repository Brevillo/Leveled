using System;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject]
public class LevelData
{
    [JsonProperty("lastTimeAccessed")] public DateTime lastTimeAccessed;
    [JsonProperty("gridSize")] public SimpleVector2Int gridSize;
    [JsonProperty("positions")] public SimpleVector2Int[] positions;
    [JsonProperty("gameTileIds")] public int[] gameTileIds;
    [JsonProperty("linkingGroups")] public string[] linkingGroups;
}

public struct SimpleVector2Int
{
    public int x;
    public int y;

    public SimpleVector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static implicit operator SimpleVector2Int(Vector2Int vector) => new(vector.x, vector.y);
    public static implicit operator Vector2Int(SimpleVector2Int simple) => new(simple.x, simple.y);
}