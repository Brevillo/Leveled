using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DictionaryGrid : Grid<TileData>
{
    private readonly Dictionary<Vector2Int, TileData> dictionary = new();
    
    public override RectInt Rect
    {
        get
        {
            Vector2Int min = Vector2Int.one * int.MaxValue;
            Vector2Int max = Vector2Int.one * int.MinValue;
        
            foreach (var position in dictionary.Keys)
            {
                min = Vector2Int.Min(min, position);
                max = Vector2Int.Max(max, position);
            }

            return new(min, max - min);
        }
    }
    
    public override IEnumerable<TileData> AllTiles => dictionary.Values;
    
    public override IEnumerable<(Vector2Int position, TileData tileData)> AllTilePositions => dictionary
        .Select(kv => (kv.Key, kv.Value));

    public override bool TryGetTile(Vector2Int position, out TileData tileData) => dictionary.TryGetValue(position, out tileData);

    public override void SetTile(Vector2Int position, TileData tileData)
    {
        if (tileData.IsEmpty)
        {
            dictionary.Remove(position);
        }
        else
        {
            dictionary[position] = tileData;
        }
    }
}