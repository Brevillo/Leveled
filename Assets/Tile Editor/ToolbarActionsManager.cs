using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolbarActionsManager : MonoBehaviour
{
    [Serializable]
    public class Blackboard
    {
        public SpaceUtility spaceUtility;
        public LinkingGroupSetter linkingGroupSetter;
        public TileEditorState editorState;
        public TilePlacer tilePlacer;

        public bool selectionOutlineActive;
        public BoundsInt selection;
        
        public Vector3 hoverSelectionCenter;
        public Vector2 hoverSelectionSize;
        public bool hoverSelectionActive;
    }

    [Header("Hover Selection")]
    [SerializeField] private Image selectionOutline;
    [SerializeField] private Image hoverSelection;
    [SerializeField] private float hoverSelectionSpeed;
    [Header("Input")]
    [SerializeField] private InputActionReference primaryToolInput;
    [SerializeField] private InputActionReference secondaryToolInput;
    [Header("References")]
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private LinkingGroupSetter linkingGroupSetter;
    [SerializeField] private TilePlacer tilePlacer;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameStateManager gameStateManager;
    
    private Blackboard blackboard;
    
    private Vector3 hoverSelectionPositionVelocity;
    private Vector2 hoverSelectionSizeVelocity;

    private bool primaryPressed;
    private bool secondaryPressed;
    
    private ToolbarAction recentBrushType;
    private ToolbarAction activeToolbarAction;
    
    public enum ToolSide
    {
        None,
        Primary,
        Secondary,
    }
    
    private void OnEnable()
    {
        gameStateManager.GameStateChanged += OnGameStateChanged;
        blackboard.editorState.EditorChanged += OnEditorChanged;
        
        primaryToolInput.action.performed += PrimaryToolDown;
        primaryToolInput.action.canceled += PrimaryToolUp;

        secondaryToolInput.action.performed += SecondaryToolDown;
        secondaryToolInput.action.canceled += SecondaryToolUp;
    }

    private void OnDisable()
    {
        gameStateManager.GameStateChanged -= OnGameStateChanged;
        blackboard.editorState.EditorChanged -= OnEditorChanged;
        
        primaryToolInput.action.performed -= PrimaryToolDown;
        primaryToolInput.action.canceled -= PrimaryToolUp;

        secondaryToolInput.action.performed -= SecondaryToolDown;
        secondaryToolInput.action.canceled -= SecondaryToolUp;
    }

    private void Awake()
    {
        blackboard = new()
        {
            spaceUtility = spaceUtility,
            linkingGroupSetter = linkingGroupSetter,
            editorState = editorState,
            tilePlacer = tilePlacer,
        };
    }

    private void OnGameStateChanged(GameState gameState)
    {
        selectionOutline.gameObject.SetActive(false);
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case ToolbarChangeInfo toolbarChangeInfo:

                if (toolbarChangeInfo.newValue == toolbarChangeInfo.previousValue) break;
                
                // Transition to new tool
                if (activeToolbarAction != null) activeToolbarAction.Deactivate();
                
                activeToolbarAction = toolbarChangeInfo.newValue;
                
                if (activeToolbarAction != null) activeToolbarAction.Activate(blackboard);

                // Save recent brush type
                if (toolbarChangeInfo.newValue is (BrushToolAction and not EraserToolAction) or RectBrushToolAction)
                {
                    recentBrushType = toolbarChangeInfo.newValue;
                }
                
                break;
            
            case PaletteChangeInfo:

                blackboard.editorState.ActiveTool = recentBrushType;
                
                break;
        }
    }
    
    #region Tool Actions
    
    private void PrimaryToolDown(InputAction.CallbackContext context)
    {
        if (blackboard.editorState.PointerOverUI)
        {
            return;
        }

        primaryPressed = true;
        activeToolbarAction?.InputDown(ToolSide.Primary);
    }

    private void SecondaryToolDown(InputAction.CallbackContext context)
    {
        if (blackboard.editorState.PointerOverUI)
        {
            return;
        }

        secondaryPressed = true;
        activeToolbarAction?.InputDown(ToolSide.Secondary);
    }

    private void PrimaryToolUp(InputAction.CallbackContext context)
    {
        primaryPressed = false;
        activeToolbarAction?.InputReleased(ToolSide.Primary);
    }

    private void SecondaryToolUp(InputAction.CallbackContext context)
    {
        secondaryPressed = false;
        activeToolbarAction?.InputReleased(ToolSide.Secondary);
    }
    
    #endregion
    
    private void Update()
    {
        activeToolbarAction?.Update();
        
        if (primaryPressed)
        {
            activeToolbarAction?.InputPressed(ToolSide.Primary);
        }

        if (secondaryPressed)
        {
            activeToolbarAction?.InputPressed(ToolSide.Secondary);
        }
        
        // Selection outline
        var selectionOutlineTransform = selectionOutline.rectTransform;
        Vector2 selectionOutlineSize = blackboard.spaceUtility.CellToCanvas(blackboard.selection.max, selectionOutlineTransform) -
                                       blackboard.spaceUtility.CellToCanvas(blackboard.selection.min, selectionOutlineTransform);
        selectionOutlineTransform.sizeDelta = selectionOutlineSize;
        selectionOutlineTransform.position = blackboard.spaceUtility.WorldToCanvas(
            blackboard.spaceUtility.GetBoundsIntCenterWorld(blackboard.selection),
            selectionOutlineTransform);
        
        selectionOutline.gameObject.SetActive(blackboard.selectionOutlineActive);

        // Hover Selection
        Vector3 hoverCenter = blackboard.hoverSelectionCenter;
        Vector2 hoverSize = blackboard.hoverSelectionSize;

        var hoverSelectionTransform = hoverSelection.rectTransform;
        hoverSize =
            blackboard.spaceUtility.WorldToCanvas((Vector2)hoverCenter + hoverSize / 2f,
                hoverSelectionTransform) -
            blackboard.spaceUtility.WorldToCanvas((Vector2)hoverCenter - hoverSize / 2f,
                hoverSelectionTransform);
        hoverCenter = blackboard.spaceUtility.WorldToCanvas(hoverCenter, hoverSelectionTransform);
        
        bool hoverSelectionActive =
            Cursor.visible &&
            !blackboard.editorState.PointerOverUI &&
            blackboard.hoverSelectionActive;
        
        hoverSelection.gameObject.SetActive(hoverSelectionActive);

        hoverSelectionTransform.sizeDelta = hoverSelectionActive
            ? Vector2.SmoothDamp(hoverSelectionTransform.sizeDelta, hoverSize, ref hoverSelectionSizeVelocity,
                hoverSelectionSpeed)
            : hoverSize;

        hoverSelection.transform.position = hoverSelectionActive
            ? Vector3.SmoothDamp(hoverSelection.transform.position, hoverCenter,
                ref hoverSelectionPositionVelocity, hoverSelectionSpeed)
            : hoverCenter;
    }
}
