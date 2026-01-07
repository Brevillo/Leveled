using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface IGrid<T>
{
    int Count { get; }
    IEnumerable<Vector2Int> AllPositions { get; }
    IEnumerable<T> AllTiles { get; }
    IEnumerable<(Vector2Int position, T tileData)> AllTilePositions { get; }
    bool TryGetTile(Vector2Int position, out T tileData);
    void SetTile(Vector2Int position, T tileData);
}

public class ArrayGrid<T> : IGrid<T>
{
    public ArrayGrid(Vector2Int size, Vector2Int anchor)
    {
        this.anchor = anchor;
        grid = new T[size.x][];

        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new T[size.y];
        }
    }

    // Grid[ x ][ y ]
    private T[][] grid;
    private Vector2Int anchor;

    public int Width => grid.Length;
    public int Height => grid.Length > 0 ? grid[0].Length : 0;
    public Vector2Int Size => new(Width, Height);

    public Vector2Int Anchor => anchor;

    private Vector2Int GridToLocal(Vector2Int position) => position - anchor;

    private bool WithinBounds(Vector2Int local) =>
        local.x >= 0
        && local.x < Width
        && local.y >= 0
        && local.y < Height;

    public bool TryGetTile(Vector2Int position, out T tileData)
    {
        Vector2Int local = GridToLocal(position);

        bool withinBounds = WithinBounds(local);

        tileData = withinBounds
            ? grid[local.x][local.y]
            : default;

        return withinBounds;
    }

    public void SetTile(Vector2Int position, T tileData)
    {
        Vector2Int local = GridToLocal(position);

        Debug.Log(local);

        if (!WithinBounds(local))
        {
            ResizeToInclude(local);
            local = GridToLocal(position);
        }

        grid[local.x][local.y] = tileData;
    }

    public int Count => Width * Height;

    public IEnumerable<Vector2Int> AllPositions
    {
        get
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    yield return new(x, y);
                }
            }
        }
    }

    public IEnumerable<T> AllTiles => AllPositions
        .Select(position => grid[position.x][position.y]);

    public IEnumerable<(Vector2Int position, T tileData)> AllTilePositions => AllPositions
        .Select(position => (position, grid[position.x][position.y]));

    private static int its;
    
    private void ResizeToInclude(Vector2Int local)
    {
        Vector2Int newMin = Vector2Int.Min(local, anchor);
        Vector2Int newMax = Vector2Int.Max(local, anchor + Size);

        // 1. Compute new dimensions
        Vector2Int newSize = newMax - newMin;

        // 2. Early-out if nothing actually changed
        if (newSize.x == Width &&
            newSize.y == Height &&
            newMin.x == anchor.x &&
            newMin.y == anchor.y)
            return;

        Debug.Log(newSize);
        
        // 3. Allocate new grid storage
        var newCells = new T[newSize.x][];

        // 4. For each column in the new grid
        for (int newGridX = 0; newGridX < newSize.x; newGridX++)
        {
            its++;
            Debug.Log(its);
            Debug.Break();
            
            // Allocate the column
            var newColumn = new T[newSize.y];
            newCells[newGridX] = newColumn;

            // Convert this row’s index into a world X coordinate
            int worldX = newMin.x + newGridX;

            // Convert world X into old-grid X index
            int oldGridX = worldX - anchor.x;

            // 5. If this column doesn't overlap the old grid horizontally
            if (oldGridX >= Width || oldGridX < 0) continue;
            
            var oldColumn = grid[oldGridX];

            // Determine overlapping Y-range in world space
            int overlapWorldYStart = Math.Max(anchor.y, newMin.y);
            int overlapWorldYEnd = Math.Min(anchor.y + Height, newMax.y);

            int overlapHeight = overlapWorldYEnd - overlapWorldYStart;

            // 6. Only copy if overlapping tiles
            if (overlapHeight <= 0) continue;
            
            // Convert world X positions into grid indices
            int oldGridY = overlapWorldYStart - anchor.y;
            int newGridY = overlapWorldYStart - newMin.y;

            if (false)
            {
                return;
            }
            
            Array.Copy(
                sourceArray: oldColumn,
                sourceIndex: oldGridY,
                destinationArray: newColumn,
                destinationIndex: newGridY,
                length: overlapHeight
            );
        }

        grid = newCells;
        anchor = new(newMin.x, newMin.y);
    }
}

public class DictionaryGrid : IGrid<TileData>
{
    private readonly Dictionary<Vector2Int, TileData> dictionary = new();

    public int Count => dictionary.Count;

    public IEnumerable<Vector2Int> AllPositions => dictionary.Keys;
    
    public IEnumerable<TileData> AllTiles => dictionary.Values;
    
    public IEnumerable<(Vector2Int position, TileData tileData)> AllTilePositions => dictionary
        .Select(kv => (kv.Key, kv.Value));

    public bool TryGetTile(Vector2Int position, out TileData tileData) => dictionary.TryGetValue(position, out tileData);

    public void SetTile(Vector2Int position, TileData tileData)
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

public class Level
{
    private IGrid<TileData> layer = new DictionaryGrid();
    // new ResizableGrid<TileData>(Vector2Int.one * 1000, Vector2Int.one * -500);

    private Dictionary<Guid, IGrid<TileData>> layers = new();

    private static IGrid<TileData> GetNewLayer() => new DictionaryGrid();

    public void SetTile(Vector2Int position, TileData tileData, Guid layerID)
    {
        if (!layers.TryGetValue(layerID, out var layer))
        {
            layer = GetNewLayer();
            layers.Add(layerID, layer);
        }
        
        layer.SetTile(position, tileData);
    }

    #region Getters
    
    public TileData GetTile(Vector2Int position)
    {
        foreach (var layer in layers.Values)
        {
            if (layer.TryGetTile(position, out var tileData))
            {
                return tileData;
            }
        }

        return default;
    }

    private IEnumerable<TileData> AllTiles => layers.Values.SelectMany(layer => layer.AllTiles);
    private IEnumerable<Vector2Int> AllPositions => layers.Values.SelectMany(layer => layer.AllPositions);
    private IEnumerable<(Vector2Int position, TileData tileData)> AllTilePositions =>
        layers.Values.SelectMany(layer => layer.AllTilePositions);

    public IEnumerable<string> AllLinkingGroups => AllTiles
        .GroupBy(tile => tile.linkingGroup)
        .Select(group => group.Key);

    public IEnumerable<Vector2Int> EntityPositions => AllTilePositions
        .Where(tile => tile.tileData.gameTile.HasEntity)
        .Select(tile => tile.position);
    
    #endregion

    public ChangeInfo ClearAllTilesChangeInfo
    {
        get
        {
            var positions = AllPositions.ToArray();
            
            return new TileChangeInfo(
                positions,
                AllTiles.ToArray(),
                new TileData[positions.Length],
                "Clear all tiles");
        }
    }

    public ChangeInfo SetLevelData(LevelData levelData, GameTilePalette palette)
    {
        var positions = levelData.positions.Select(simple => (Vector2Int)simple).ToArray();
        var tiles = Enumerable.Range(0, levelData.positions.Length)
            .Select(i => new TileData(palette.GetTile(levelData.gameTileIds[i]), levelData.linkingGroups[i]))
            .ToArray();

        return new TileChangeInfo(positions,
            positions.Select(GetTile).ToArray(),
            tiles,
            "Filled new level");
    }

    public LevelData GetData() => new()
    {
        positions = layer.AllPositions.Select(position => (SimpleVector2Int)position).ToArray(),
        gameTileIds = layer.AllTiles.Select(tileData => tileData.gameTile.ID).ToArray(),
        linkingGroups = layer.AllTiles.Select(tileData => tileData.linkingGroup).ToArray(),
    };
}

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Tile Editor State")]
public class TileEditorState : GameService
{
    [SerializeField] private Changelog changelog;

    private Guid layerID = Guid.NewGuid();
    private readonly Level level = new Level();

    public void SetTiles(Vector2Int[] positions, TileData[] tiles, string description) =>
        changelog.SendChange(new TileChangeInfo(positions, positions.Select(Level.GetTile).ToArray(), tiles, description));

    public Level Level => level;

    #region Changelog
    
    protected override void Initialize()
    {
        changelog.StateUpdated += OnStateUpdated;
    }

    protected override void InstanceDestroyed()
    {
        changelog.StateUpdated -= OnStateUpdated;
    }

    private void OnStateUpdated(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case TileChangeInfo tileChangeInfo:

                for (int i = 0; i < tileChangeInfo.positions.Length; i++)
                {
                    Level.SetTile(tileChangeInfo.positions[i], tileChangeInfo.newTiles[i], layerID);
                }

                break;
        }
    }
    
    #endregion
    
    #region Saving
    
    public void ClearAllTiles() => changelog.SendChange(level.ClearAllTilesChangeInfo);
    
    public void SetLevelData(LevelData levelData, GameTilePalette palette)
    {
        // Clear current level
        ClearAllTiles();
        
        // Fill new level
        changelog.SendChange(Level.SetLevelData(levelData, palette));
        
        // Clear changelog
        changelog.ClearChangelog();
    }
    
    #endregion
}
