using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EditorActionSceneBindings : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private List<EditorActionBinding> bindings;

    private void OnEnable()
    {
        if (bindings == null) return;

        foreach (var binding in bindings)
        {
            binding.Bind(gameStateManager);
        }
    }

    private void OnDisable()
    {
        if (bindings == null) return;

        foreach (var binding in bindings)
        {
            binding.Unbind();
        }
    }

    private void OnValidate()
    {
        if (bindings == null) return;
        
        foreach (var binding in bindings)
        {
            binding.OnValidate();
        }
    }

    [Serializable]
    private class EditorActionBinding
    {
        [HideInInspector, SerializeField] private string name;
        
        [SerializeField] private EditorAction editorAction;
        [SerializeField] private UnityEvent binding;

        public void OnValidate()
        {
            name = editorAction != null ? editorAction.name : "No Action!";
        }
        
        public void Bind(GameStateManager gameStateManager)
        {
            editorAction.Enable(gameStateManager);
            editorAction.Action += binding.Invoke;
        }

        public void Unbind()
        {
            editorAction.Disable();
            editorAction.Action -= binding.Invoke;
        }
    }
}