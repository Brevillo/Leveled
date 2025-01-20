using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ToolbarActionsManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Grid grid;
    [SerializeField] private LinkingGroupSetter linkingGroupSetter;
    [SerializeField] private SpriteRenderer selectionOutline;
    [SerializeField] private SpriteRenderer hoverSelection;
    [SerializeField] private float hoverSelectionSpeed;
    [Header("Input")]
    [SerializeField] private InputActionReference primaryToolInput;
    [SerializeField] private InputActionReference secondaryToolInput;
    [Header("References")]
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameStateManager gameStateManager;
    
    private Vector3Int dragStart;
    private BoundsInt selection;
    private State state;
    private ToolSide toolSide;

    private Vector3 hoverSelectionPositionVelocity;
    private Vector2 hoverSelectionSizeVelocity;

    private ToolType recentBrushType = ToolType.Brush;

    private List<Vector3Int> brushedTiles = new();
    
    private enum State
    {
        None,
        DrawingRect,
        Selecting,
        Selected,
        MovingSelection,
    }

    private enum ToolSide
    {
        None,
        Primary,
        Secondary,
    }
    
    private Vector3Int MouseCell => grid.WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition));
    
    private void OnEnable()
    {
        gameStateManager.GameStateChanged += OnGameStateChanged;
        editorState.EditorChanged += OnEditorChanged;
        
        primaryToolInput.action.performed += PrimaryToolDown;
        primaryToolInput.action.canceled += PrimaryToolUp;

        secondaryToolInput.action.performed += SecondaryToolDown;
        secondaryToolInput.action.canceled += SecondaryToolUp;
    }

    private void OnDisable()
    {
        gameStateManager.GameStateChanged -= OnGameStateChanged;
        editorState.EditorChanged -= OnEditorChanged;
        
        primaryToolInput.action.performed -= PrimaryToolDown;
        primaryToolInput.action.canceled -= PrimaryToolUp;

        secondaryToolInput.action.performed -= SecondaryToolDown;
        secondaryToolInput.action.canceled -= SecondaryToolUp;
    }

    private void OnGameStateChanged(GameState gameState)
    {
        state = State.None;

        selectionOutline.gameObject.SetActive(false);
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case ToolbarChangeInfo toolbarChangeInfo:

                if (toolbarChangeInfo.previousTool == ToolType.Selection
                    && toolbarChangeInfo.newTool != ToolType.Selection)
                {
                    state = State.None;
                }

                if (toolbarChangeInfo.newTool is ToolType.Brush or ToolType.RectBrush)
                {
                    recentBrushType = toolbarChangeInfo.newTool;
                }
                
                break;
        }
    }
    
    #region Tool Actions
    
    private void PrimaryToolDown(InputAction.CallbackContext context)
    {
        if (editorState.PointerOverUI || toolSide == ToolSide.Secondary)
        {
            return;
        }

        toolSide = ToolSide.Primary;
            
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:
                
                editorState.StartChangeBundle("Brushed multiple tiles.");
                brushedTiles.Clear();
                
                break;
            
            case ToolType.Eraser:
                
                editorState.StartChangeBundle("Erased multiple tiles.");
                
                break;
            
            case ToolType.RectBrush:
       
                dragStart = MouseCell;
                state = State.DrawingRect;
                
                break;
            
            case ToolType.Selection:

                dragStart = MouseCell;

                switch (state)
                {
                    case State.None:

                        state = State.Selecting;
                        
                        break;
                    
                    case State.Selected:

                        if (selection.Contains(dragStart))
                        {
                            state = State.MovingSelection;
                        }
                        
                        break;
                }
                
                if (state != State.MovingSelection || !selection.Contains(MouseCell))
                {
                    state = State.Selecting;
                }

                break;
        }
    }

    private void SecondaryToolDown(InputAction.CallbackContext context)
    {
        if (editorState.PointerOverUI || toolSide == ToolSide.Primary)
        {
            return;
        }

        toolSide = ToolSide.Secondary;
        
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:
                
                editorState.StartChangeBundle("Brushed multiple tiles.");
                
                break;
            
            case ToolType.Eraser:

                editorState.StartChangeBundle("Erased multiple tiles.");
                
                break;
            
            case ToolType.RectBrush:
                
                dragStart = MouseCell;
                state = State.DrawingRect;
                
                break;
        }
    }

    private void PrimaryToolPressed()
    {
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:
                
                Vector3Int brushPosition = MouseCell;
                
                editorState.SetTile(brushPosition, new(editorState.ActiveTile));
                brushedTiles.Add(brushPosition);
                
                break;
                
            case ToolType.Eraser:
                editorState.SetTile(MouseCell, TileData.Empty);
                break;
                
            case ToolType.Picker:
                editorState.ActiveTile = editorState.GetTile(MouseCell).gameTile;
                break;
        }
    }

    private void SecondaryToolPressed()
    {
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:
            case ToolType.Eraser:
                editorState.SetTile(MouseCell, TileData.Empty);
                break;
        }
    }

    private void PrimaryToolUp(InputAction.CallbackContext context)
    {
        if (toolSide != ToolSide.Primary)
        {
            return;
        }
        
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:
                
                if (editorState.ActiveTile.Linkable)
                {
                    GetLinkingGroup(linkingGroup =>
                    {
                        var tiles = new TileData[brushedTiles.Count];
                        Array.Fill(tiles, new TileData(editorState.ActiveTile, linkingGroup));
                        
                        editorState.SetTiles(brushedTiles.ToArray(), tiles);
                        editorState.EndChangeBundle();
                    });
                }
                else
                {
                    editorState.EndChangeBundle();
                }
                
                break;
            
            case ToolType.Eraser:

                editorState.EndChangeBundle();
                
                break;
            
            case ToolType.RectBrush:
                
                if (state == State.DrawingRect)
                {
                    SetTilemapRect(editorState.ActiveTile);
                }

                state = State.None;
                
                break;
                
            case ToolType.Picker:
                editorState.ActiveTool = recentBrushType;
                break;
                
            case ToolType.Selection:

                switch (state)
                {
                    case State.Selecting:
                        
                        Vector3Int min = Vector3Int.Min(dragStart, MouseCell);
                        Vector3Int max = Vector3Int.Max(dragStart, MouseCell);

                        selection = new(min, max - min + Vector3Int.one);

                        state = State.Selected;
                        
                        break;
                        
                    case State.MovingSelection:

                        state = State.Selected;

                        if (dragStart == MouseCell) break;

                        List<Vector3Int> originPositions = new();
                        foreach (var position in selection.allPositionsWithin)
                        {
                            originPositions.Add(position);
                        }

                        var nullTiles = new TileData[originPositions.Count];

                        var originTiles = originPositions
                            .Select(editorState.GetTile)
                            .ToArray();

                        Vector3Int delta = MouseCell - dragStart;
                        var destinationPositions = originPositions
                            .Select(position => position + delta)
                            .ToArray();

                        editorState.StartChangeBundle("Moved tile selection");

                        editorState.SetTiles(originPositions.ToArray(), nullTiles);
                        editorState.SetTiles(destinationPositions, originTiles);

                        editorState.EndChangeBundle();

                        selection.position += delta;

                        break;
                }
                
                break;

            default:
                state = State.None;
                break;
        }

        toolSide = ToolSide.None;
    }

    private void SecondaryToolUp(InputAction.CallbackContext context)
    {
        if (toolSide != ToolSide.Secondary)
        {
            return;
        }
        
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:
                
                editorState.EndChangeBundle();
                
                break;
            
            case ToolType.Eraser:

                editorState.EndChangeBundle();
                
                break;

            case ToolType.RectBrush:

                if (state == State.DrawingRect)
                {
                    SetTilemapRect(null);
                }

                state = State.None;
                
                break;
            
            default:
                state = State.None;
                break;
        }

        toolSide = ToolSide.None;
    }
    
    #endregion
    
    private void Update()
    {
        switch (toolSide)
        {
            case ToolSide.Primary:
                PrimaryToolPressed();
                break;
            
            case ToolSide.Secondary:
                SecondaryToolPressed();
                break;
        }
        
        // Selection outline
        selectionOutline.size = (Vector2Int)selection.size;
        selectionOutline.transform.position =
            (grid.GetCellCenterWorld(selection.min) + grid.GetCellCenterWorld(selection.max)) / 2f -
            new Vector3(0.5f, 0.5f, 0);
        
        selectionOutline.gameObject.SetActive(state is State.Selected or State.MovingSelection);

        // Hover Selection
        Vector3Int mouseCell = MouseCell;
        Vector3 mouseWorld = grid.GetCellCenterWorld(mouseCell);

        (Vector3 position, Vector2 size) = state switch
        {
            State.DrawingRect or State.Selecting => (
                position: (mouseWorld + grid.GetCellCenterWorld(dragStart)) / 2f,
                size: new Vector2(
                    Mathf.Abs(mouseCell.x - dragStart.x) + 1,
                    Mathf.Abs(mouseCell.y - dragStart.y) + 1)),

            State.MovingSelection => (
                position: selection.center + mouseWorld - grid.GetCellCenterWorld(dragStart),
                size: (Vector2)(Vector3)selection.size),
            
            _ => (
                position: mouseWorld,
                size: Vector2.one),
        };

        hoverSelection.size = Vector2.SmoothDamp(hoverSelection.size, size, ref hoverSelectionSizeVelocity,
            hoverSelectionSpeed);
        hoverSelection.transform.localPosition =
            Vector3.SmoothDamp(hoverSelection.transform.localPosition, position, ref hoverSelectionPositionVelocity,
                hoverSelectionSpeed);
        bool hoverSelectionActive =
            (!editorState.PointerOverUI || state is State.DrawingRect or State.Selecting or State.MovingSelection) &&
            editorState.ActiveTool != ToolType.Mover; 
        hoverSelection.gameObject.SetActive(hoverSelectionActive);
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

        var tiles = new TileData[positions.Length];
        Array.Fill(tiles, new(tile));
        
        if (tile != null && tile.Linkable)
        {
            editorState.StartChangeBundle("Set multiple tiles");
            
            editorState.SetTiles(positions, tiles);

            GetLinkingGroup(linkingGroup =>
            {
                var tiles = new TileData[positions.Length];
                Array.Fill(tiles, new(tile, linkingGroup));
                
                editorState.SetTiles(positions, tiles);
                editorState.EndChangeBundle();
            });
        }
        else
        {
            editorState.SetTiles(positions, tiles);
        }
    }

    private void GetLinkingGroup(Action<string> linkingGroupAction)
    {
        Vector2 mouseViewport =
            mainCamera.WorldToViewportPoint(grid.GetCellCenterWorld(MouseCell) + Vector3.down * 0.5f); 
        Vector2 position = (Vector2.one - mouseViewport) * mainCamera.rect.min + mouseViewport;
        
        linkingGroupSetter.GetLinkingGroupAtPosition(position, linkingGroupAction);
    }
}
