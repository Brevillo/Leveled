using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Grid grid;
    // [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private SpriteRenderer selectionOutline;

    private Dictionary<Tilemap, Tilemap> tilemaps;
    
    private bool mouseDown;
    private Vector3Int dragStart;
    private BoundsInt selection;
    private PlacerState state;
    
    public enum PlacerState
    {
        None,
        DrawingRect,
        Selecting,
        MovingSelection,
    }
    
    public Vector3Int DragStart => dragStart;
    public PlacerState State => state;
    public Tilemap[] Tilemaps => tilemaps.Values.ToArray();
    
    private void Awake()
    {
        tilemaps = new();
    }

    private void OnEnable()
    {
        editorState.LevelLoaded += OnLevelLoaded;
    }

    private void OnDisable()
    {
        editorState.LevelLoaded -= OnLevelLoaded;
    }

    private void OnLevelLoaded(LevelData levelData)
    {
        var tilePositions = Enumerable.Range(0, levelData.positions.Length)
            .Select(i => (levelData.positions[i], editorState.Palette.Tiles[levelData.ids[i]]))
            .ToArray();

        foreach (var tilemap in tilemaps.Values)
        {
            tilemap.ClearAllTiles();
        }        
        
        SetTiles(tilePositions);
    }

    private void Update()
    {
        if (editorState.GameState == GameState.Playing)
        {
            state = PlacerState.None;
            mouseDown = false;

            selectionOutline.gameObject.SetActive(false);
            
            return;
        }

        Vector3Int mouseCell = grid.WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        
        if (PointerOverUI() && !mouseDown)
        {
            mouseDown = false;
            return;
        }
        
        // Primary Down
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;

            if (state != PlacerState.Selecting && state != PlacerState.MovingSelection || editorState.ActiveTool != ToolType.Selection)
            {
                state = PlacerState.None;
            }
            
            switch (editorState.ActiveTool)
            {
                case ToolType.RectBrush:
                    dragStart = mouseCell;
                    state = PlacerState.DrawingRect;
                    break;
                
                case ToolType.Selection:

                    dragStart = mouseCell;

                    if (state != PlacerState.MovingSelection || !selection.Contains(mouseCell))
                    {
                        state = PlacerState.Selecting;
                    }

                    break;
            }
        }
        
        // Secondary Down
        if (Input.GetMouseButtonDown(1))
        {
            mouseDown = true;
            
            switch (editorState.ActiveTool)
            {
                case ToolType.RectBrush:
                    state = PlacerState.DrawingRect;
                    dragStart = mouseCell;
                    break;
            }
        }

        // Primary Pressed
        if (Input.GetMouseButton(0))
        {
            switch (editorState.ActiveTool)
            {
                case ToolType.Brush:
                    SetTile(mouseCell, editorState.ActiveTile);
                    break;
                
                case ToolType.Eraser:
                    SetTile(mouseCell, null);
                    break;
                
                case ToolType.Picker:
                    editorState.ActiveTile = GetTile(mouseCell);
                    break;
            }
        }

        // Secondary Pressed
        if (Input.GetMouseButton(1))
        {
            switch (editorState.ActiveTool)
            {
                case ToolType.Brush:
                case ToolType.Eraser:
                    SetTile(mouseCell, null);
                    break;
            }
        }

        // Primary Released
        if (Input.GetMouseButtonUp(0) && mouseDown)
        {
            mouseDown = false;
            switch (editorState.ActiveTool)
            {
                case ToolType.RectBrush:
                    SetTilemapRect(editorState.ActiveTile);
                    break;
                
                case ToolType.Picker:
                    editorState.ActiveTool = ToolType.Brush;
                    break;
                
                case ToolType.Selection:

                    switch (state)
                    {
                        case PlacerState.Selecting:
                            
                            if (dragStart != mouseCell)
                            {
                                Vector3Int min = Vector3Int.Min(dragStart, mouseCell);
                                Vector3Int max = Vector3Int.Max(dragStart, mouseCell);

                                selection = new(min, max - min + Vector3Int.one);
                                
                                state = PlacerState.MovingSelection;
                            }
                            else
                            {
                                state = PlacerState.None;
                            }
                            
                            break;
                        
                        case PlacerState.MovingSelection:
                            MoveSelection();
                            break;
                    }
                    
                    break;
            }
        }

        if (Input.GetMouseButtonUp(1) && mouseDown)
        {
            mouseDown = false;
            switch (editorState.ActiveTool)
            {
                case ToolType.RectBrush:
                    SetTilemapRect(null);
                    break;
            }
        }
        
        // Selection outline
        if (state == PlacerState.MovingSelection)
        {
            selectionOutline.gameObject.SetActive(true);
            
            selectionOutline.size = (Vector2Int)selection.size;

            selectionOutline.transform.position =
                (grid.GetCellCenterWorld(selection.min) + grid.GetCellCenterWorld(selection.max)) / 2f - new Vector3(0.5f, 0.5f, 0);
        }
        else
        {
            selectionOutline.gameObject.SetActive(false);
        }
        
        void SetTilemapRect(GameTile tile)
        {
            Vector3Int start = Vector3Int.Min(dragStart, mouseCell);
            Vector3Int end = Vector3Int.Max(dragStart, mouseCell);
            Vector3Int size = end - start + new Vector3Int(1, 1, 0);

            var positions = Enumerable.Range(0, size.x * size.y)
                .Select(i => new Vector3Int(start.x + i % size.x, start.y + i / size.x))
                .ToArray();
            
            SetTiles(positions
                .Select(position => (position, tile))
                .ToArray());

            state = PlacerState.None;
        }

        void MoveSelection()
        {
            var fromPositions = ToEnumerable(selection.allPositionsWithin).ToArray();
            
            Vector3Int delta = mouseCell - dragStart;
            var toTiles = fromPositions
                .Select(position => (position + delta, GetTile(position)))
                .ToArray();
            
            var holeTiles = fromPositions
                .Select(position => (position, (GameTile)null))
                .ToArray();
            
            SetTiles(holeTiles);
            SetTiles(toTiles);

            selection.position += delta;
        }
    }

    private IEnumerable<T> ToEnumerable<T>(IEnumerator<T> enumerator)
    {
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }

    public bool PointerOverUI()
    {
        int uiLayer = LayerMask.NameToLayer("UI");
        
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition,
        };
        var results = new List<RaycastResult>();
        
        EventSystem.current.RaycastAll(eventData, results);

        return results.Any(result => result.gameObject.layer == uiLayer);
    }

    private Tilemap GetTilemap(GameTile gameTile)
    {
        if (gameTile == null || gameTile.TilemapPrefab == null)
        {
            return null;
        }
        
        if (tilemaps.TryGetValue(gameTile.TilemapPrefab, out var tilemap)) return tilemap;
        
        tilemap = Instantiate(gameTile.TilemapPrefab, grid.transform);
        tilemaps[gameTile.TilemapPrefab] = tilemap;

        return tilemap;
    }

    private GameTile GetTile(Vector3Int position) => editorState.GetTile(position);
    
    private void SetTile(Vector3Int position, GameTile tile)
    {
        editorState.SetTile(position, tile);
        var selfTilemap = GetTilemap(tile); 
        
        if (selfTilemap != null)
        {
            selfTilemap.SetTile(position, tile.TileBase);
        }
        
        foreach (var tilemap in tilemaps.Values)
        {
            if (tilemap == selfTilemap) continue;
            
            tilemap.SetTile(position, null);
        }
    }

    private void SetTiles((Vector3Int position, GameTile tile)[] tilePositions)
    {
        foreach (var tilePosition in tilePositions)
        {
            editorState.SetTile(tilePosition.position, tilePosition.tile);
        }
        
        var tilemapGroups = tilePositions
            .Select(item => (item.position, item.tile, tilemap: GetTilemap(item.tile)))
            .GroupBy(item => item.tilemap)
            .ToArray();

        var allTilemaps = tilemaps.Values.ToArray();
        
        foreach (var group in tilemapGroups)
        {
            var positionArray = group.Select(item => item.position).ToArray();
            var nullTiles = new TileBase[positionArray.Length];
            Array.Fill(nullTiles, null);
            
            if (group.Key != null)
            {
                var tileArray = group.Select(item => item.tile.TileBase).ToArray();
                group.Key.SetTiles(positionArray, tileArray);
                
            }
            
            foreach (var tilemap in allTilemaps)
            {
                if (tilemap == group.Key) continue;
                
                tilemap.SetTiles(positionArray, nullTiles);
            }
        }
    }
}
