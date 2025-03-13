using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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
    [SerializeField] private ToolbarActionPalette toolbarActionPalette;
    [SerializeField] private ToolbarBlackboard blackboard;
    [SerializeField] private GameStateManager gameStateManager;
    [Space]
    [SerializeField] private ToolInput primaryToolInput;
    [SerializeField] private ToolInput secondaryToolInput;
    [SerializeField] private ToolInput tertiaryToolInput;
    [Space]
    [Header("References")]
    [SerializeField] private TilePlacer tilePlacer;
    [SerializeField] private LinkingGroupSetter linkingGroupSetter;

    private ToolbarAction recentBrushType;
    
    private void Awake()
    {
        blackboard.tilePlacer = tilePlacer;
        blackboard.linkingGroupSetter = linkingGroupSetter;

        foreach (var toolbarAction in toolbarActionPalette.Actions)
        {
            toolbarAction.InjectReferences(blackboard);
        }

        primaryToolInput.Init(this, ToolSide.Primary);
        secondaryToolInput.Init(this, ToolSide.Secondary);
        tertiaryToolInput.Init(this, ToolSide.Tertiary);
    }

    private void OnEnable()
    {
        primaryToolInput.Enable();
        secondaryToolInput.Enable();
        tertiaryToolInput.Enable();
        
        editorState.EditorChanged += OnEditorChanged;
    }

    private void OnDisable()
    {
        primaryToolInput.Disable();
        secondaryToolInput.Disable();
        tertiaryToolInput.Disable();
        
        editorState.EditorChanged -= OnEditorChanged;
    }
    
    private void Update()
    {
        if (gameStateManager.GameState != GameState.Editing) return;
        
        if (editorState.ActiveTool != null)
        {
            editorState.ActiveTool.Update();
        }

        primaryToolInput.Update();
        secondaryToolInput.Update();
        tertiaryToolInput.Update();
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
                    toolbarChangeInfo.newValue.Activate();
                }

                // Save recent brush type
                if (toolbarChangeInfo.newValue is BrushToolAction or RectBrushToolAction or FillToolAction)
                {
                    recentBrushType = toolbarChangeInfo.newValue;
                }
                
                break;
            
            case PaletteChangeInfo:

                editorState.ActiveTool = recentBrushType;
                
                break;
        }
    }

    [Serializable]
    private class ToolInput
    {
        [SerializeField] private InputActionReference toolInput;
        [SerializeField] private ToolbarAction overrideAction;

        private ToolbarActionsManager manager;
        private ToolSide toolSide;
        
        private bool pressed;

        private ToolbarAction Action => overrideAction != null
            ? overrideAction
            : manager.editorState.ActiveTool;

        public void Init(ToolbarActionsManager manager, ToolSide toolSide)
        {
            this.manager = manager;
            this.toolSide = toolSide;
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
            if (manager.editorState.PointerOverUI || manager.gameStateManager.GameState != GameState.Editing) return;

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
            if (manager.gameStateManager.GameState != GameState.Editing) return;
            
            pressed = false;

            if (Action != null)
            {
                Action.InputReleased(toolSide);
            }
        }
    }
}
