using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArrayGrid<T> : Grid<T>
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

    public Vector2Int Anchor => anchor;

    private Vector2Int GridToLocal(Vector2Int position) => position - anchor;

    private bool WithinBounds(Vector2Int local) =>
        local.x >= 0
        && local.x < Rect.size.x
        && local.y >= 0
        && local.y < Rect.size.y;
    
    public override RectInt Rect => new(anchor, new(grid.Length, grid.Length > 0 ? grid[0].Length : 0));

    public override bool TryGetTile(Vector2Int position, out T tileData)
    {
        Vector2Int local = GridToLocal(position);

        bool withinBounds = WithinBounds(local);

        tileData = withinBounds
            ? grid[local.x][local.y]
            : default;

        return withinBounds;
    }

    public override void SetTile(Vector2Int position, T tileData)
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

    public override IEnumerable<T> AllTiles => ((Grid<T>)this).AllPositions
        .Select(position => grid[position.x][position.y]);

    public override IEnumerable<(Vector2Int position, T tileData)> AllTilePositions => AllPositions
        .Select(position => (position, grid[position.x][position.y]));

    private static int its;
    
    private void ResizeToInclude(Vector2Int local)
    {
        var rect = Rect;
        Vector2Int oldMin = rect.min;
        Vector2Int size = rect.size;
        Vector2Int newMin = Vector2Int.Min(local, oldMin);
        Vector2Int newMax = Vector2Int.Max(local, oldMin + size);

        // 1. Compute new dimensions
        Vector2Int newSize = newMax - newMin;

        // 2. Early-out if nothing actually changed
        if (newSize.x == size.x &&
            newSize.y == size.y &&
            newMin.x == oldMin.x &&
            newMin.y == oldMin.y)
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
            int oldGridX = worldX - oldMin.x;

            // 5. If this column doesn't overlap the old grid horizontally
            if (oldGridX >= size.x || oldGridX < 0) continue;
            
            var oldColumn = grid[oldGridX];

            // Determine overlapping Y-range in world space
            int overlapWorldYStart = Math.Max(oldMin.y, newMin.y);
            int overlapWorldYEnd = Math.Min(oldMin.y + size.y, newMax.y);

            int overlapHeight = overlapWorldYEnd - overlapWorldYStart;

            // 6. Only copy if overlapping tiles
            if (overlapHeight <= 0) continue;
            
            // Convert world X positions into grid indices
            int oldGridY = overlapWorldYStart - oldMin.y;
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
        oldMin = new(newMin.x, newMin.y);
    }
}