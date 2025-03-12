using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public enum ToolSide
{
    None,
    Primary,
    Secondary,
    Tertiary,
}

[CreateAssetMenu(menuName = "Leveled/Toolbar Actions/Toolbar Actions Manager", order = 0)]
public class ToolbarActionsManager : GameService
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private ToolbarBlackboard blackboard;
    [Space]
    [SerializeField] private ToolInput primaryToolInput;
    [SerializeField] private ToolInput secondaryToolInput;
    [SerializeField] private ToolInput tertiaryToolInput;
    [Space]
    [SerializeField] private List<ToolbarAction> toolbarActions;

    private ToolbarAction recentBrushType;
    
    protected override void Initialize()
    {
        primaryToolInput.Enable(this, ToolSide.Primary);
        secondaryToolInput.Enable(this, ToolSide.Secondary);
        tertiaryToolInput.Enable(this, ToolSide.Tertiary);
        
        foreach (var toolbarAction in toolbarActions)
        {
            toolbarAction.InjectReferences(blackboard);
        }

        editorState.EditorChanged += OnEditorChanged;
    }

    protected override void Update()
    {
        if (editorState.ActiveTool != null)
        {
            editorState.ActiveTool.Update();
        }

        primaryToolInput.Update();
        secondaryToolInput.Update();
        tertiaryToolInput.Update();
    }

    protected override void InstanceDestroyed()
    {
        primaryToolInput.Disable();
        secondaryToolInput.Disable();
        tertiaryToolInput.Disable();

        editorState.EditorChanged -= OnEditorChanged;
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
                if (toolbarChangeInfo.newValue is 
                    BrushToolAction and not EraserToolAction or RectBrushToolAction or FillToolAction)
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
        
        public void Enable(ToolbarActionsManager manager, ToolSide toolSide)
        {
            this.manager = manager;
            this.toolSide = toolSide;
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
}
