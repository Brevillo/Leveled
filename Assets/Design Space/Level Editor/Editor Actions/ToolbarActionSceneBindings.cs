using System;
using System.Collections.Generic;
using UnityEngine;

public class ToolbarActionSceneBindings : MonoBehaviour
{
    [SerializeField] private ChangeloggedToolbarAction activeTool;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private List<Binding> bindings;

    private void OnEnable()
    {
        foreach (var binding in bindings)
        {
            binding.Bind(gameStateManager, activeTool);
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
        
        private ChangeloggedToolbarAction activeTool;

        public void Bind(GameStateManager gameStateManager, ChangeloggedToolbarAction activeTool)
        {
            this.activeTool = activeTool;
            
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
            activeTool.Value = toolbarAction;
        }

        public void OnValidate()
        {
            name = editorAction != null ? editorAction.name : "No Action!";
        }
    }
}