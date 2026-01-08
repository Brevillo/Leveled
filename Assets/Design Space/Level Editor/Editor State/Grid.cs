using System.Collections.Generic;
using UnityEngine;

public abstract class Grid<T>
{
    public abstract RectInt Rect { get; }
    
    public IEnumerable<Vector2Int> AllPositions
    {
        get
        {
            var rect = Rect;

            for (int x = rect.xMin; x < rect.xMax + 1; x++)
            {
                for (int y = rect.yMin; y < rect.yMax + 1; y++)
                {
                    yield return new(x, y);
                }
            }
        }
    }

    public abstract IEnumerable<T> AllTiles { get; }
    public abstract IEnumerable<(Vector2Int position, T tileData)> AllTilePositions { get; }
    public abstract bool TryGetTile(Vector2Int position, out T tileData);
    public T GetTileOrDefault(Vector2Int position) => TryGetTile(position, out var tileData) ? tileData : default;
    public abstract void SetTile(Vector2Int position, T tileData);
}