using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public readonly struct LinkingGroup
{
    public readonly string groupID;
    
    public LinkingGroup(string groupID)
    {
        this.groupID = groupID;
    }

    public static Dictionary<Vector2Int, Vector2Int> GetAllConnections(LevelInstance levelInstance)
    {
        Dictionary<string, List<Vector2Int>> linkedPositions = new();
        
        foreach (var position in levelInstance.EntityPositions)
        {
            var metadata = levelInstance.GetTileOnAnyLayer(position).metadata;
            if (metadata != null && metadata.TryGetValue<LinkingGroup>(out var linkingGroup))
            {
                if (!linkedPositions.TryGetValue(linkingGroup.groupID, out var positions))
                {
                    positions = new();
                    linkedPositions.Add(linkingGroup.groupID, positions);
                }
                
                positions.Add(position);
            }
        }

        return new(linkedPositions
            .SelectMany(group => group.Value
                .Select(position => new KeyValuePair<Vector2Int, Vector2Int>(position, group.Value
                    .Where(other => other != position)
                    .OrderBy(other => ((Vector2)(other - position)).sqrMagnitude)
                    .DefaultIfEmpty(position)
                    .First()))));
    }
}

public enum TileRotation
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}

public readonly struct EnumStruct<T>
{
    public readonly T value;

    public EnumStruct(T value)
    {
        this.value = value;
    }
}

public readonly struct TileData
{
    public readonly GameTile gameTile;
    public readonly Metadata metadata;

    public T GetMetaData<T>() => metadata == null ? default : metadata.GetValueOrDefault<T>();
    
    public TileData(GameTile gameTile, params object[] metaData)
    {
        this.gameTile = gameTile;
        this.metadata = new();

        foreach (var entry in metaData)
        {
            this.metadata.SetValue(entry);
        }
    }

    public TileData(GameTile gameTile, Metadata metadata)
    {
        this.gameTile = gameTile;
        this.metadata = metadata;
    }
    
    public bool IsEmpty => gameTile == null || gameTile.IsNullTileBase;

    public TileData SetMetaData(object entry)
    {
        var newMetadata = metadata?.GetCopy() ?? new();
        
        newMetadata.SetValue(entry);
        
        return new(gameTile, newMetadata);
    }
}
