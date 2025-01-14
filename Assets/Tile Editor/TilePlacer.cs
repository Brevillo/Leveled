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
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameStateManager gameStateManager;
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

    private Vector3Int MouseCell => grid.WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition));
    
    private void Awake()
    {
        tilemaps = new();
    }

    private void OnEnable()
    {
        editorState.EditorChanged += OnEditorChanged;
    }

    private void OnDisable()
    {
        editorState.EditorChanged -= OnEditorChanged;
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case TileChangeInfo tileChangeInfo:

                var position = tileChangeInfo.position;
                var tile = tileChangeInfo.newTile;
                var selfTilemap = GetTilemap(tile);
        
                if (tile != null)
                {
                    selfTilemap.SetTile(position, tile.TileBase);
                }
        
                foreach (var tilemap in tilemaps.Values)
                {
                    if (tilemap == selfTilemap) continue;
            
                    tilemap.SetTile(position, null);
                }
                break;
            
            case MultiTileChangeInfo multiTileChangeInfo:
                
                var tilemapGroups = Enumerable.Range(0, multiTileChangeInfo.positions.Length)
                    .Select(i =>
                    {
                        var position = multiTileChangeInfo.positions[i];
                        var tile = multiTileChangeInfo.newTiles[i];
                        var tilemap = GetTilemap(tile);
                        
                        return (position, tile, tilemap);
                    })
                    .GroupBy(item => item.tilemap)
                    .ToArray();
                
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
                    
                    foreach (var tilemap in tilemaps.Values)
                    {
                        if (tilemap == group.Key) continue;
                
                        tilemap.SetTiles(positionArray, nullTiles);
                    }
                }
                
                break;
        }
    }

    private void Update()
    {
        if (gameStateManager.GameState == GameState.Playing)
        {
            state = PlacerState.None;
            mouseDown = false;

            selectionOutline.gameObject.SetActive(false);
            
            return;
        }

        Vector3Int mouseCell = MouseCell;
        
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
                    editorState.SetTile(mouseCell, editorState.ActiveTile);
                    break;
                
                case ToolType.Eraser:
                    editorState.SetTile(mouseCell, null);
                    break;
                
                case ToolType.Picker:
                    editorState.ActiveTile = editorState.GetTile(mouseCell);
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
                    editorState.SetTile(mouseCell, null);
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
    }
    
    private void SetTilemapRect(GameTile tile)
    {
        Vector3Int dragEnd = MouseCell;
        Vector3Int min = Vector3Int.Min(dragStart, dragEnd);
        Vector3Int max = Vector3Int.Max(dragStart, dragEnd);
        Vector3Int size = max - min + new Vector3Int(1, 1, 0);

        var positions = Enumerable.Range(0, size.x * size.y)
            .Select(i => new Vector3Int(min.x + i % size.x, min.y + i / size.x))
            .ToArray();

        var tiles = new GameTile[positions.Length];
        Array.Fill(tiles, tile);
        
        editorState.SetTiles(positions, tiles);

        state = PlacerState.None;
    }

    private void MoveSelection()
    {
        var originPositions = ToEnumerable(selection.allPositionsWithin).ToArray();

        var nullTiles = new GameTile[originPositions.Length];
        Array.Fill(nullTiles, null);
        
        var originTiles = originPositions
            .Select(editorState.GetTile)
            .ToArray();
        
        Vector3Int delta = MouseCell - dragStart;
        var destinationPositions = originPositions
            .Select(position => position + delta)
            .ToArray();

        var deleteTiles = editorState.GetMultiTileChangeInfo(originPositions, nullTiles);
        var moveTiles = editorState.GetMultiTileChangeInfo(destinationPositions, originTiles);
        var bundle = new ChangeInfoBundle("Moved tile selection", new[]
        {
            deleteTiles,
            moveTiles,
        });
        
        editorState.SendEditorChange(bundle);

        selection.position += delta;
    }

    private static IEnumerable<T> ToEnumerable<T>(IEnumerator<T> enumerator)
    {
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }

    private static bool PointerOverUI()
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

        if (tilemaps.TryGetValue(gameTile.TilemapPrefab, out var tilemap))
        {
            return tilemap;
        }
        
        tilemap = Instantiate(gameTile.TilemapPrefab, grid.transform);
        tilemaps[gameTile.TilemapPrefab] = tilemap;

        return tilemap;
    }
}
