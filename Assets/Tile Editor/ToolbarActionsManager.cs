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
    private State currentState;
    private ToolSide toolSide;

    private Vector3 hoverSelectionPositionVelocity;
    private Vector2 hoverSelectionSizeVelocity;
    
    public enum State
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

    public Vector3Int DragStart => dragStart;

    public BoundsInt Selection => selection;
    
    public State CurrentState => currentState;

    private Vector3Int MouseCell => grid.WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition));

    private static bool PointerOverUI
    {
        get
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
    
    private void OnEnable()
    {
        gameStateManager.GameStateChanged += OnGameStateChanged;
        
        primaryToolInput.action.performed += PrimaryToolDown;
        primaryToolInput.action.canceled += PrimaryToolUp;

        secondaryToolInput.action.performed += SecondaryToolDown;
        secondaryToolInput.action.canceled += SecondaryToolUp;
    }

    private void OnDisable()
    {
        gameStateManager.GameStateChanged -= OnGameStateChanged;
        
        primaryToolInput.action.performed -= PrimaryToolDown;
        primaryToolInput.action.canceled -= PrimaryToolUp;

        secondaryToolInput.action.performed -= SecondaryToolDown;
        secondaryToolInput.action.canceled -= SecondaryToolUp;
    }

    private void OnGameStateChanged(GameState gameState)
    {
        currentState = State.None;

        selectionOutline.gameObject.SetActive(false);
    }
    
    #region Tool Actions
    
    private void PrimaryToolDown(InputAction.CallbackContext context)
    {
        if (PointerOverUI || toolSide == ToolSide.Secondary)
        {
            return;
        }

        toolSide = ToolSide.Primary;
            
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
                currentState = State.DrawingRect;
                
                break;
            
            case ToolType.Selection:

                dragStart = MouseCell;

                switch (currentState)
                {
                    case State.None:

                        currentState = State.Selecting;
                        
                        break;
                    
                    case State.Selected:

                        if (selection.Contains(dragStart))
                        {
                            currentState = State.MovingSelection;
                        }
                        
                        break;
                }
                
                if (currentState != State.MovingSelection || !selection.Contains(MouseCell))
                {
                    currentState = State.Selecting;
                }

                break;
        }
    }

    private void SecondaryToolDown(InputAction.CallbackContext context)
    {
        if (PointerOverUI || toolSide == ToolSide.Primary)
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
                currentState = State.DrawingRect;
                
                break;
        }
    }

    private void PrimaryToolPressed()
    {
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:
                editorState.SetTile(MouseCell, editorState.ActiveTile);
                break;
                
            case ToolType.Eraser:
                editorState.SetTile(MouseCell, null);
                break;
                
            case ToolType.Picker:
                editorState.ActiveTile = editorState.GetTile(MouseCell);
                break;
        }
    }

    private void SecondaryToolPressed()
    {
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:
            case ToolType.Eraser:
                editorState.SetTile(MouseCell, null);
                break;
        }
    }

    private void PrimaryToolUp(InputAction.CallbackContext context)
    {
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:
                
                editorState.EndChangeBundle();
                
                break;
            
            case ToolType.Eraser:

                editorState.EndChangeBundle();
                
                break;
            
            case ToolType.RectBrush:
                
                if (currentState == State.DrawingRect)
                {
                    SetTilemapRect(editorState.ActiveTile);
                }

                currentState = State.None;
                
                break;
                
            case ToolType.Picker:
                editorState.ActiveTool = ToolType.Brush;
                break;
                
            case ToolType.Selection:

                switch (currentState)
                {
                    case State.Selecting:

                        if (dragStart == MouseCell)
                        {
                            currentState = State.None;
                            break;
                        }
                        
                        Vector3Int min = Vector3Int.Min(dragStart, MouseCell);
                        Vector3Int max = Vector3Int.Max(dragStart, MouseCell);

                        selection = new(min, max - min + Vector3Int.one);

                        currentState = State.Selected;
                        
                        break;
                        
                    case State.MovingSelection:
                        
                        if (dragStart != MouseCell)
                        {
                            MoveSelection();
                        }

                        currentState = State.Selected;
                        
                        break;
                    
                    default:
                        currentState = State.None;
                        break;
                }
                
                break;

            default:
                currentState = State.None;
                break;
        }

        toolSide = ToolSide.None;
    }

    private void SecondaryToolUp(InputAction.CallbackContext context)
    {
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:
                
                editorState.EndChangeBundle();
                
                break;
            
            case ToolType.Eraser:

                editorState.EndChangeBundle();
                
                break;

            case ToolType.RectBrush:

                if (currentState == State.DrawingRect)
                {
                    SetTilemapRect(null);
                }

                currentState = State.None;
                
                break;
            
            default:
                currentState = State.None;
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
        
        selectionOutline.gameObject.SetActive(currentState is State.Selected or State.MovingSelection);

        // Hover Selection
        Vector3Int mouseCell = MouseCell;
        Vector3 mouseWorld = grid.GetCellCenterWorld(mouseCell);

        (Vector3 position, Vector2 size) = currentState switch
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
        hoverSelection.gameObject.SetActive(
            !PointerOverUI || currentState is State.DrawingRect or State.Selecting or State.MovingSelection);
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

        editorState.StartChangeBundle("Moved tile selection");
        
        editorState.SetTiles(originPositions, nullTiles);
        editorState.SetTiles(destinationPositions, originTiles);
        
        editorState.EndChangeBundle();
        
        selection.position += delta;
    }

    private static IEnumerable<T> ToEnumerable<T>(IEnumerator<T> enumerator)
    {
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }
}
