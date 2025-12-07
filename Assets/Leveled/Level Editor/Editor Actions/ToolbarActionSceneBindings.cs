using System;
using System.Collections.Generic;
using UnityEngine;

public class ToolbarActionSceneBindings : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private List<Binding> bindings;

    private void OnEnable()
    {
        foreach (var binding in bindings)
        {
            binding.Bind(gameStateManager, editorState);
        }
    }

    private void OnDisable()
    {
        foreach (var binding in bindings)
        {
            binding.Unbind();
        }
    }

    private void OnValidate()
    {
        foreach (var binding in bindings)
        {
            binding.OnValidate();
        }
    }

    [Serializable]
    private class Binding
    {
        [HideInInspector, SerializeField] private string name;
        [SerializeField] private EditorAction editorAction;
        [SerializeField] private ToolbarAction toolbarAction;
        
        private TileEditorState editorState;

        public void Bind(GameStateManager gameStateManager, TileEditorState editorState)
        {
            this.editorState = editorState;
            
            editorAction.Enable(gameStateManager);
            editorAction.Action += OnAction;
        }

        public void Unbind()
        {
            editorAction.Disable();
            editorAction.Action -= OnAction;
        }
        
        private void OnAction()
        {
            editorState.ActiveTool = toolbarAction;
        }

        public void OnValidate()
        {
            name = editorAction != null ? editorAction.name : "No Action!";
        }
    }
}