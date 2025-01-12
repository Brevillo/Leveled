using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private SpriteRenderer selectionOutline;

    private Vector3Int? dragStart;
    private bool mouseDown;

    private BoundsInt selection;
    private bool selecting;

    private Vector3 moveStart;
    
    public Vector3Int? DragStart => dragStart;
    
    private void Awake()
    {
        dragStart = null;
        selecting = false;
    }

    private void Update()
    {
        Vector3Int mouseCell = tilemap.WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition));

        print(selection.Contains(mouseCell));
        
        if (PointerOverUI() && !mouseDown)
        {
            mouseDown = false;
            return;
        }
        
        // Primary Down
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;

            if (editorState.ActiveTool != ToolType.Selection)
            {
                selecting = false;
            }
            
            switch (editorState.ActiveTool)
            {
                case ToolType.RectBrush:
                    dragStart = mouseCell;
                    break;
                
                case ToolType.Selection:

                    if (selecting && selection.Contains(mouseCell))
                    {
                        selecting = true;
                        moveStart = mouseCell;
                    }
                    else
                    {
                        dragStart = mouseCell;
                        selecting = false;
                    }
                    
                    break;
            }
        }
        
        // Secondary Down
        if (Input.GetMouseButtonDown(1))
        {
            mouseDown = true;
            selecting = false;
            
            switch (editorState.ActiveTool)
            {
                case ToolType.RectBrush:
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
                    tilemap.SetTile(mouseCell, editorState.ActiveTile);
                    break;
                
                case ToolType.Eraser:
                    tilemap.SetTile(mouseCell, null);
                    break;
                
                case ToolType.Picker:
                    editorState.ActiveTile = tilemap.GetTile(mouseCell);
                    break;
            }
        }

        // Secondary Presed
        if (Input.GetMouseButton(1))
        {
            switch (editorState.ActiveTool)
            {
                case ToolType.Brush:
                case ToolType.Eraser:
                    tilemap.SetTile(mouseCell, null);
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

                    // Move Selection
                    if (selecting)
                    {
                        
                    }
                    
                    // Build Selection
                    else
                    {
                        Vector3Int start = dragStart ?? default;

                        if (start != mouseCell)
                        {
                            Vector3Int min = Vector3Int.Min(start, mouseCell);
                            Vector3Int max = Vector3Int.Max(start, mouseCell);

                            selection = new(min, max - min + Vector3Int.one);
                            Debug.Log(selection.position + " : " + selection.size);
                            selecting = true;
                        }
                        
                        dragStart = null;
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
        selectionOutline.gameObject.SetActive(selecting);
        
        if (selecting)
        {
            selectionOutline.size = (Vector2Int)selection.size;

            selectionOutline.transform.position =
                (tilemap.GetCellCenterWorld(selection.min) + tilemap.GetCellCenterWorld(selection.max)) / 2f - new Vector3(0.5f, 0.5f, 0);
        }
        
        void SetTilemapRect(TileBase tile)
        {
            Vector3Int rectStart = dragStart ?? default;
            Vector3Int start = Vector3Int.Min(rectStart, mouseCell);
            Vector3Int end = Vector3Int.Max(rectStart, mouseCell);
            Vector3Int size = end - start + new Vector3Int(1, 1, 0);

            var positions = Enumerable.Range(0, size.x * size.y)
                .Select(i => new Vector3Int(start.x + i % size.x, start.y + i / size.x))
                .ToArray();
            
            var tiles = new TileBase[positions.Length];
            Array.Fill(tiles, tile);
            
            tilemap.SetTiles(positions, tiles);

            dragStart = null;
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
}
