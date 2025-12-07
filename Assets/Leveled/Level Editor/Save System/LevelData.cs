using System;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject]
public class LevelData
{
    [JsonProperty("lastTimeAccessed")] public DateTime lastTimeAccessed;
    [JsonProperty("gridSize")] public Vector2Int gridSize;
    [JsonProperty("positions")] public Vector2Int[] positions;
    [JsonProperty("gameTileIds")] public int[] gameTileIds;
    [JsonProperty("linkingGroups")] public string[] linkingGroups;
}
