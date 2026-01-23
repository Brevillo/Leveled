using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.InputSystem;

public enum ToolSide
{
    None,
    Primary,
    Secondary,
    Tertiary,
}

public class ToolbarActionsManager : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private Changelog changelog;
    [SerializeField] private ChangeloggedToolbarAction activeTool;
    [SerializeField] private ToolbarActionPalette toolbarActionPalette;
    [SerializeField] private ToolbarBlackboard blackboard;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private ToolbarAction defaultBrushType;
    [Space]
    [SerializeField] private ToolInput primaryToolInput;
    [SerializeField] private ToolInput secondaryToolInput;
    [SerializeField] private ToolInput tertiaryToolInput;
    [Space]
    [Header("References")]
    [SerializeField] private TilePlacer tilePlacer;
    
    private ToolbarAction recentBrushType;
    
    private void Awake()
    {
        blackboard.tilePlacer = tilePlacer;
        blackboard.ResetValues();

        recentBrushType = defaultBrushType;
        
        foreach (var toolbarAction in toolbarActionPalette.Actions)
        {
            toolbarAction.InjectReferences(blackboard);
        }

        var toolInputs = new List<ToolInput>
        {
            primaryToolInput,
            secondaryToolInput,
            tertiaryToolInput,
        };
        
        primaryToolInput.Init(this, toolInputs, ToolSide.Primary);
        secondaryToolInput.Init(this, toolInputs, ToolSide.Secondary);
        tertiaryToolInput.Init(this, toolInputs, ToolSide.Tertiary);
    }

    private void OnDestroy()
    {
        foreach (var toolbarAction in toolbarActionPalette.Actions)
        {
            toolbarAction.Cleanup();
        }
    }

    private void OnEnable()
    {
        primaryToolInput.Enable();
        secondaryToolInput.Enable();
        tertiaryToolInput.Enable();
        
        changelog.StateUpdated += OnStateUpdated;
        gameStateManager.EditorStateChanged += OnEditorStateChanged;
    }

    private void OnEditorStateChanged(EditorState editorState)
    {
        switch (editorState)
        {
            case EditorState.Playing:

                
                
                break;
        }
    }

    private void OnDisable()
    {
        primaryToolInput.Disable();
        secondaryToolInput.Disable();
        tertiaryToolInput.Disable();
        
        changelog.StateUpdated -= OnStateUpdated;
    }
    
    private void Update()
    {
        if (gameStateManager.EditorState != EditorState.Editing) return;
        
        if (activeTool.Value != null)
        {
            activeTool.Value.Update();
        }

        primaryToolInput.Update();
        secondaryToolInput.Update();
        tertiaryToolInput.Update();
    }

    private void LateUpdate()
    {
        if (gameStateManager.EditorState != EditorState.Editing) return;
        
        primaryToolInput.LateUpdate();
        secondaryToolInput.LateUpdate();
        tertiaryToolInput.LateUpdate();
    }

    private void OnStateUpdated(ChangeInfo changeInfo)
    {
        if (activeTool.ThisChangeInfo(changeInfo, out var toolbarChangeInfo))
        {
            // Deactivate old tool and activate new
            if (toolbarChangeInfo.previousValue != null)
            {
                toolbarChangeInfo.previousValue.Deactivate();
            }
            
            if (toolbarChangeInfo.newValue != null)
            {
                toolbarChangeInfo.newValue.Activate();
            }

            // Save recent brush type
            if (toolbarChangeInfo.newValue is BrushToolAction or RectBrushToolAction or FillToolAction)
            {
                recentBrushType = toolbarChangeInfo.newValue;
            }

            blackboard.Deselect();
        }
        
        switch (changeInfo)
        {
            case ValueChangeInfo<GameTile>:

                activeTool.Value = recentBrushType;
                
                break;
        }
    }

    [Serializable]
    private class ToolInput
    {
        [SerializeField] private InputActionReference toolInput;
        [SerializeField] private ToolbarAction overrideAction;
        [SerializeField] private bool allowOverViewportLayer;
        
        private ToolbarActionsManager manager;
        private ToolSide toolSide;
        [NonSerialized]
        private List<ToolInput> allToolInputs;
        
        private bool pressed;
        
        private ToolbarAction Action => overrideAction != null
            ? overrideAction
            : manager.activeTool.Value;

        public void Init(ToolbarActionsManager manager, List<ToolInput> allToolInputs, ToolSide toolSide)
        {
            this.manager = manager;
            this.toolSide = toolSide;
            this.allToolInputs = allToolInputs.Where(input => input != this).ToList();
        }
        
        public void Enable()
        {
            toolInput.action.performed += OnToolDown;
            toolInput.action.canceled += OnToolReleased;
        }

        public void Disable()
        {
            toolInput.action.performed -= OnToolDown;
            toolInput.action.canceled -= OnToolReleased;
        }

        private void OnToolDown(InputAction.CallbackContext context)
        {
            var uiLayer = UIUtility.PointerOverUILayer;
            if (uiLayer == UILayer.Default
                || (uiLayer == UILayer.Viewport && !allowOverViewportLayer)
               || manager.gameStateManager.EditorState != EditorState.Editing
               || allToolInputs.Exists(input => input.pressed))
            {
                return;
            }

            pressed = true;
            
            if (Action != null)
            {
                Action.InputDown(toolSide);
            }
        }

        public void Update()
        {
            if (manager.gameStateManager.EditorState != EditorState.Editing)
            {
                pressed = false;
                return;
            }
            
            if (pressed && Action != null)
            {
                Action.InputPressed(toolSide);
            }
        }

        public void LateUpdate()
        {
            if (manager.gameStateManager.EditorState == EditorState.Editing
                && pressed
                && Action != null)
            {
                Action.InputPressedLate(toolSide);
            }
        }

        private void OnToolReleased(InputAction.CallbackContext context)
        {
            if (manager.gameStateManager.EditorState != EditorState.Editing) return;
            
            pressed = false;

            if (Action != null)
            {
                Action.InputReleased(toolSide);
            }
        }
    }
}
