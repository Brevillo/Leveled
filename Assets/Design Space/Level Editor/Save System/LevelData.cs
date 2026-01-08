using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject]
public class LevelData
{
    [JsonProperty("lastTimeAccessed")] public DateTime lastTimeAccessed;
    
    [JsonProperty("gridSize", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public SimpleVector2Int gridSize;
    [JsonProperty("positions", DefaultValueHandling = DefaultValueHandling.Ignore)] 
    public SimpleVector2Int[] positions;
    [JsonProperty("gameTileIds", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int[] gameTileIds;
    [JsonProperty("linkingGroups", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string[] linkingGroups;
    
    [JsonProperty("layers")] public LevelLayerData[] layers;
}

[JsonObject(MemberSerialization.OptIn)]
public class LevelLayerData
{
    [JsonProperty("layerID")] public Guid layerID;
    [JsonProperty("gridSize")] public SimpleVector2Int gridSize;
    [JsonProperty("gameTileIds")] public int[] gameTileIds;
    [JsonProperty("metaData")] public Dictionary<string, TileMetaData> metaData;
    
    public Vector2Int[] AllPositions => Enumerable.Range(0, gridSize.x * gridSize.y)
        .Select(i => new Vector2Int(i / gridSize.y, i % gridSize.y))
        .ToArray();
}

[JsonObject(MemberSerialization.OptIn)]
public class TileMetaData
{
    [JsonProperty("entries",
        ItemTypeNameHandling = TypeNameHandling.All,
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    private object[] Entries
    {
        get => typedEntries?.Values.ToArray();
        set => typedEntries = value.ToDictionary(entry => entry.GetType());
    }
    
    private Dictionary<Type, object> typedEntries = new();

    public int Count => typedEntries?.Count ?? 0;
    
    public T GetValueOrDefault<T>() => (T)typedEntries?.GetValueOrDefault(typeof(T));
    public void SetValue(object value)
    {
        if (typedEntries == null) return;
        
        typedEntries[value.GetType()] = value;
    }
}

public struct SimpleVector2Int : IEquatable<SimpleVector2Int>
{
    public int x;
    public int y;
    
    public SimpleVector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString() => $"{x},{y}";
    
    public static SimpleVector2Int Parse(string s)
    {
        var parts = s.Split(',');
        return new SimpleVector2Int(int.Parse(parts[0]), int.Parse(parts[1]));
    }
    
    public static implicit operator Vector2Int(SimpleVector2Int vector) => new(vector.x, vector.y);
    public static implicit operator SimpleVector2Int(Vector2Int vector) => new(vector.x, vector.y);
    
    public static SimpleVector2Int FromVector2Int(Vector2Int vector) => new(vector.x, vector.y);
    public static Vector2Int ToVector2Int(SimpleVector2Int vector) => new(vector.x, vector.y);

    public bool Equals(SimpleVector2Int other) => x == other.x && y == other.y;
    public override bool Equals(object obj) => obj is SimpleVector2Int other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(x, y);
}