using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolbarActionsManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private LinkingGroupSetter linkingGroupSetter;
    [SerializeField] private Image selectionOutline;
    [SerializeField] private Image hoverSelection;
    [SerializeField] private float hoverSelectionSpeed;
    [SerializeField] private TilePlacer tilePlacer;
    [Header("Input")]
    [SerializeField] private InputActionReference primaryToolInput;
    [SerializeField] private InputActionReference secondaryToolInput;
    [SerializeField] private InputActionReference copyInput;
    [SerializeField] private InputActionReference pasteInput;
    [Header("References")]
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameStateManager gameStateManager;
    
    private Vector3Int dragStart;
    private BoundsInt selection;
    private State state;
    private ToolSide toolSide;

    private bool inverseBrushMode;
    
    private bool selectionCopied;
    private Vector3Int clipboardPosition;
    private List<(Vector3Int position, TileData tileData)> clipboard = new();

    private Vector3 hoverSelectionPositionVelocity;
    private Vector2 hoverSelectionSizeVelocity;

    private ToolType recentBrushType = ToolType.Brush;
    
    private List<Vector3Int> brushedTiles = new();

    private GameTile DrawingTile => inverseBrushMode ? null : toolSide switch
    {
        ToolSide.Primary => editorState.PrimaryTile,
        ToolSide.Secondary => editorState.SecondaryTile,
        _ => null,
    };
    
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
    
    private void OnEnable()
    {
        gameStateManager.GameStateChanged += OnGameStateChanged;
        editorState.EditorChanged += OnEditorChanged;
        
        primaryToolInput.action.performed += PrimaryToolDown;
        primaryToolInput.action.canceled += PrimaryToolUp;

        secondaryToolInput.action.performed += SecondaryToolDown;
        secondaryToolInput.action.canceled += SecondaryToolUp;

        copyInput.action.performed += Copy;
        pasteInput.action.performed += Paste;
    }

    private void OnDisable()
    {
        gameStateManager.GameStateChanged -= OnGameStateChanged;
        editorState.EditorChanged -= OnEditorChanged;
        
        primaryToolInput.action.performed -= PrimaryToolDown;
        primaryToolInput.action.canceled -= PrimaryToolUp;

        secondaryToolInput.action.performed -= SecondaryToolDown;
        secondaryToolInput.action.canceled -= SecondaryToolUp;

        copyInput.action.performed -= Copy;
        pasteInput.action.performed -= Paste;
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

                if (toolbarChangeInfo.previousValue == ToolType.Selection
                    && toolbarChangeInfo.newValue != ToolType.Selection)
                {
                    state = State.None;
                    selectionCopied = false;
                }

                if (toolbarChangeInfo.newValue is ToolType.Brush or ToolType.RectBrush)
                {
                    recentBrushType = toolbarChangeInfo.newValue;
                }
                
                break;
            
            case PaletteChangeInfo:

                editorState.ActiveTool = recentBrushType;
                
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
            case ToolType.Eraser:
                
                brushedTiles.Clear();

                inverseBrushMode = editorState.GetTile(spaceUtility.MouseCell).gameTile == editorState.PrimaryTile;
                
                break;
            
            case ToolType.RectBrush:
       
                dragStart = spaceUtility.MouseCell;
                state = State.DrawingRect;
                
                break;
            
            case ToolType.Selection:

                dragStart = spaceUtility.MouseCell;
                selectionCopied = false;

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
                
                if (state != State.MovingSelection || !selection.Contains(spaceUtility.MouseCell))
                {
                    state = State.Selecting;
                }

                break;
            
            case ToolType.Linker:

                state = State.Selecting;
                dragStart = spaceUtility.MouseCell;
                
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
            case ToolType.Eraser:

                brushedTiles.Clear();
                
                inverseBrushMode = editorState.GetTile(spaceUtility.MouseCell).gameTile == editorState.SecondaryTile;

                break;
            
            case ToolType.RectBrush:
                
                dragStart = spaceUtility.MouseCell;
                state = State.DrawingRect;
                
                break;
        }
    }

    private void PrimaryToolPressed()
    {
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:

                tilePlacer.PlaceTile(spaceUtility.MouseCell, new(DrawingTile));
                brushedTiles.Add(spaceUtility.MouseCell);
                
                break;
                
            case ToolType.Eraser:
                
                tilePlacer.PlaceTile(spaceUtility.MouseCell, TileData.Empty);
                brushedTiles.Add(spaceUtility.MouseCell);
               
                break;
        }
    }

    private void SecondaryToolPressed()
    {
        switch (editorState.ActiveTool)
        {
            case ToolType.Brush:

                tilePlacer.PlaceTile(spaceUtility.MouseCell, new(DrawingTile));
                brushedTiles.Add(spaceUtility.MouseCell);
                
                break;

            case ToolType.Eraser:
               
                tilePlacer.PlaceTile(spaceUtility.MouseCell, TileData.Empty);
                brushedTiles.Add(spaceUtility.MouseCell);
                
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
                
                if (editorState.PrimaryTile.Linkable)
                {
                    GetLinkingGroup(linkingGroup => SetTiles(new(DrawingTile, linkingGroup)));
                }
                else
                {
                    SetTiles(new(DrawingTile));
                }
                
                break;
            
            case ToolType.Eraser:

                SetTiles(TileData.Empty);
                
                break;
            
            case ToolType.RectBrush:
                
                if (state == State.DrawingRect)
                {
                    SetTilemapRect(editorState.PrimaryTile);
                }

                state = State.None;
                
                break;
                
            case ToolType.Picker:
                
                editorState.PrimaryTile = editorState.GetTile(spaceUtility.MouseCell).gameTile;
                
                break;
                
            case ToolType.Selection:

                switch (state)
                {
                    case State.Selecting:

                        selection = CalculateSelection();

                        state = State.Selected;
                        
                        break;
                        
                    case State.MovingSelection:

                        state = State.Selected;

                        if (dragStart == spaceUtility.MouseCell) break;

                        List<Vector3Int> originPositions = new();
                        foreach (var position in selection.allPositionsWithin)
                        {
                            originPositions.Add(position);
                        }

                        var nullTiles = new TileData[originPositions.Count];

                        var originTiles = originPositions
                            .Select(editorState.GetTile)
                            .ToArray();

                        Vector3Int delta = spaceUtility.MouseCell - dragStart;
                        var destinationPositions = originPositions
                            .Select(position => position + delta)
                            .ToArray();

                        editorState.StartChangeBundle("Moved tile selection");

                        editorState.SetTiles(originPositions.ToArray(), nullTiles, "Deleted original selection");
                        editorState.SetTiles(destinationPositions, originTiles, "Filled new selection");

                        editorState.EndChangeBundle();

                        selection.position += delta;

                        break;
                }
                
                break;

            case ToolType.Linker:

                selection = CalculateSelection();

                state = State.Selected;

                var boundsIntAnchorWorld = spaceUtility.GetBoundsIntAnchorWorld(selection, new Vector2(0.5f, 0f));
                Debug.DrawLine(Vector3.zero, boundsIntAnchorWorld, Color.green, 10);
                
                linkingGroupSetter.GetLinkingGroupAtPosition(
                    boundsIntAnchorWorld,
                    linkingGroup =>
                    {
                        var modifiedPositions = new List<Vector3Int>();
                        var modifiedTiles = new List<TileData>();
                        
                        foreach (var position in selection.allPositionsWithin)
                        {
                            var tile = editorState.GetTile(position);

                            if (tile.Linkable)
                            {
                                modifiedPositions.Add(position);
                                modifiedTiles.Add(new(tile.gameTile, linkingGroup));
                            }
                        }
                        
                        editorState.SetTiles(modifiedPositions.ToArray(), modifiedTiles.ToArray(), "Changed tile linking");

                        state = State.None;
                    });
                
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
                
                if (DrawingTile != null && DrawingTile.Linkable)
                {
                    GetLinkingGroup(linkingGroup => SetTiles(new(DrawingTile, linkingGroup)));
                }
                else
                {
                    SetTiles(new(DrawingTile));
                }

                break;
                
            case ToolType.Eraser:
                
                SetTiles(TileData.Empty);
                
                break;

            case ToolType.RectBrush:

                if (state == State.DrawingRect)
                {
                    SetTilemapRect(editorState.SecondaryTile);
                }

                state = State.None;
                
                break;
            
            case ToolType.Picker:

                editorState.SecondaryTile = editorState.GetTile(spaceUtility.MouseCell).gameTile;
                
                break;
            
            default:
                state = State.None;
                selectionCopied = false;
                break;
        }

        toolSide = ToolSide.None;
    }

    private void Copy(InputAction.CallbackContext context)
    {
        if (editorState.ActiveTool != ToolType.Selection || state != State.Selected) return;

        selectionCopied = true;
        clipboardPosition = selection.position;
        
        clipboard.Clear();

        foreach (var position in selection.allPositionsWithin)
        {
            clipboard.Add((position, editorState.GetTile(position)));
        }
    }
    
    private void Paste(InputAction.CallbackContext context)
    {
        if (!selectionCopied) return;
        
        Vector3 selectionCenterOffset = spaceUtility.GetBoundsIntCenterWorld(selection);
        Vector3Int selectionCenter = spaceUtility.WorldToCell(
            spaceUtility.SnapWorldToCell((Vector2)(spaceUtility.MouseWorld - selectionCenterOffset) + Vector2.one / 2f) +
            selectionCenterOffset - (Vector3)selection.size / 2f);
        
        Vector3Int delta = selectionCenter - clipboardPosition;

        editorState.SetTiles(
            clipboard
                .Select(tile => tile.position + delta)
                .ToArray(),
            clipboard
                .Select(tile => tile.tileData)
                .ToArray(),
            "Pasted tiles");
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
        var selectionOutlineTransform = selectionOutline.rectTransform;
        Vector2 selectionOutlineSize = spaceUtility.CellToCanvas(selection.max, selectionOutlineTransform) -
                                       spaceUtility.CellToCanvas(selection.min, selectionOutlineTransform);
        selectionOutlineTransform.sizeDelta = selectionOutlineSize;
        selectionOutlineTransform.position = spaceUtility.WorldToCanvas(spaceUtility.GetBoundsIntCenterWorld(selection),
            selectionOutlineTransform);
        
        selectionOutline.gameObject.SetActive(state is State.Selected or State.MovingSelection);

        // Hover Selection
        Vector3Int mouseCell = spaceUtility.MouseCell;
        Vector3 mouseCellWorld = spaceUtility.MouseCellCenterWorld;
        Vector3 selectionCenterOffset = spaceUtility.GetBoundsIntCenterWorld(selection);

        (Vector3 hoverSelectionCenter, Vector2 hoverSelectionSize) = state switch
        {
            State.DrawingRect or State.Selecting => (
                (mouseCellWorld + spaceUtility.CellToWorld(dragStart)) / 2f,
                new Vector2(
                    Mathf.Abs(mouseCell.x - dragStart.x) + 1,
                    Mathf.Abs(mouseCell.y - dragStart.y) + 1)),

            State.MovingSelection => (
                selection.center + mouseCellWorld - spaceUtility.CellToWorld(dragStart),
                (Vector2)(Vector3)selection.size),
            
            State.Selected => selectionCopied 
                ? (spaceUtility.SnapWorldToCell((Vector2)(spaceUtility.MouseWorld - selectionCenterOffset) + Vector2.one / 2f) + selectionCenterOffset, 
                    (Vector3)selection.size)
                : (mouseCellWorld, 
                    Vector2.one),
            
            _ => (
                mouseCellWorld,
                Vector2.one),
        };

        var hoverSelectionTransform = hoverSelection.rectTransform;
        hoverSelectionSize =
            spaceUtility.WorldToCanvas((Vector2)hoverSelectionCenter + hoverSelectionSize / 2f,
                hoverSelectionTransform) -
            spaceUtility.WorldToCanvas((Vector2)hoverSelectionCenter - hoverSelectionSize / 2f,
                hoverSelectionTransform);
        hoverSelectionCenter = spaceUtility.WorldToCanvas(hoverSelectionCenter, hoverSelectionTransform);
        
        bool hoverSelectionActive =
            Cursor.visible &&
            (!editorState.PointerOverUI || state is State.DrawingRect or State.Selecting or State.MovingSelection) &&
            editorState.ActiveTool != ToolType.Mover;
        
        hoverSelection.gameObject.SetActive(hoverSelectionActive);

        hoverSelectionTransform.sizeDelta = hoverSelectionActive
            ? Vector2.SmoothDamp(hoverSelectionTransform.sizeDelta, hoverSelectionSize, ref hoverSelectionSizeVelocity,
                hoverSelectionSpeed)
            : hoverSelectionSize;

        hoverSelection.transform.position = hoverSelectionActive
            ? Vector3.SmoothDamp(hoverSelection.transform.position, hoverSelectionCenter,
                ref hoverSelectionPositionVelocity, hoverSelectionSpeed)
            : hoverSelectionCenter;
    }

    private BoundsInt CalculateSelection()
    {
        Vector3Int dragEnd = spaceUtility.MouseCell;
        Vector3Int min = Vector3Int.Min(dragStart, dragEnd);
        Vector3Int max = Vector3Int.Max(dragStart, dragEnd);

        return new(min, max - min + Vector3Int.one);
    }

    private void SetTiles(TileData tileData)
    {
        var tiles = new TileData[brushedTiles.Count];
        Array.Fill(tiles, tileData);
        
        editorState.SetTiles(brushedTiles.ToArray(), tiles, editorState.ActiveTool switch
        {
            ToolType.Brush => "Brushed tiles",
            ToolType.Eraser => "Erased tiles",
            _ => throw new ArgumentException(),
        });
        
        brushedTiles.Clear();
    }
    
    private void SetTilemapRect(GameTile tile)
    {
        Vector3Int dragEnd = spaceUtility.MouseCell;
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
            tilePlacer.PlaceTiles(positions, tiles);

            GetLinkingGroup(linkingGroup =>
            {
                var linkedTiles = new TileData[positions.Length];
                Array.Fill(linkedTiles, new(tile, linkingGroup));
                
                editorState.SetTiles(positions, linkedTiles, "Brushed tile rectangle");
            });
        }
        else
        {
            editorState.SetTiles(positions, tiles, "Brushed tile rectangle");
        }
    }

    private void GetLinkingGroup(Action<string> linkingGroupAction)
    {
        linkingGroupSetter.GetLinkingGroupAtPosition(spaceUtility.MouseCellCenterWorld + Vector3.down * 0.5f, linkingGroupAction);
    }
}
