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
    [SerializeField] private ToolInput primaryToolInput;
    [SerializeField] private ToolInput secondaryToolInput;
    [SerializeField] private ToolInput tertiaryToolInput;
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
    private bool tertiaryPressed;
    
    private ToolbarAction recentBrushType;

    [Serializable]
    private class ToolInput
    {
        [SerializeField] private InputActionReference toolInput;
        [SerializeField] private ToolSide toolSide;
        [SerializeField] private ToolbarAction overrideAction;

        private ToolbarActionsManager manager;
        
        private bool pressed;

        private ToolbarAction Action => overrideAction != null
            ? overrideAction
            : manager.editorState.ActiveTool;
        
        public void Init(ToolbarActionsManager manager)
        {
            this.manager = manager;
        }
        
        public void OnEnable()
        {
            toolInput.action.performed += OnToolDown;
            toolInput.action.canceled += OnToolReleased;
        }

        public void OnDisable()
        {
            toolInput.action.performed -= OnToolDown;
            toolInput.action.canceled -= OnToolReleased;
        }

        private void OnToolDown(InputAction.CallbackContext context)
        {
            if (manager.editorState.PointerOverUI) return;

            pressed = true;
            
            if (Action != null)
            {
                Action.InputDown(toolSide);
            }
        }

        public void Update()
        {
            if (pressed && Action != null)
            {
                Action.InputPressed(toolSide);
            }
        }

        private void OnToolReleased(InputAction.CallbackContext context)
        {
            pressed = false;

            if (Action != null)
            {
                Action.InputReleased(toolSide);
            }
        }
    }
    
    public enum ToolSide
    {
        None,
        Primary,
        Secondary,
        Tertiary,
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
        
        primaryToolInput.Init(this);
        secondaryToolInput.Init(this);
        tertiaryToolInput.Init(this);
    }
    
    private void OnEnable()
    {
        gameStateManager.GameStateChanged += OnGameStateChanged;
        blackboard.editorState.EditorChanged += OnEditorChanged;
        
        primaryToolInput.OnEnable();
        secondaryToolInput.OnEnable();
        tertiaryToolInput.OnEnable();
    }
    
    private void OnDisable()
    {
        gameStateManager.GameStateChanged -= OnGameStateChanged;
        blackboard.editorState.EditorChanged -= OnEditorChanged;
        
        primaryToolInput.OnDisable();
        secondaryToolInput.OnDisable();
        tertiaryToolInput.OnDisable();
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
                
                // Deactivate old tool and activate new
                if (toolbarChangeInfo.previousValue != null)
                {
                    toolbarChangeInfo.previousValue.Deactivate();
                }
                
                if (toolbarChangeInfo.newValue != null)
                {
                    toolbarChangeInfo.newValue.Activate(blackboard);
                }

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
    
    private void Update()
    {
        if (editorState.ActiveTool != null)
        {
            editorState.ActiveTool.Update();
        }
        
        primaryToolInput.Update();
        secondaryToolInput.Update();
        tertiaryToolInput.Update();
        
        // Selection outline
        var selectionOutlineTransform = selectionOutline.rectTransform;
        Vector2 selectionOutlineSize = blackboard.spaceUtility.CellToCanvas((Vector2Int)blackboard.selection.max, selectionOutlineTransform) -
                                       blackboard.spaceUtility.CellToCanvas((Vector2Int)blackboard.selection.min, selectionOutlineTransform);
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
