using System;
using System.Collections.Generic;
using UnityEngine;

public class ToolbarUIManager : MonoBehaviour
{
    [Serializable]
    private class ToolbarEditorAction
    {
        [HideInInspector] public string name;
        public ToolbarAction toolbarAction;
        public EditorAction editorAction;
    }
    
    [SerializeField] private List<ToolbarEditorAction> actions;
    [SerializeField] private ToolbarUIItem toolbarUIItemPrefab;
    [SerializeField] private Transform itemsParent;

    private void OnValidate()
    {
        foreach (var action in actions)
        {
            action.name = action.toolbarAction.name;
        }
    }

    private void Start()
    {
        foreach (var action in actions)
        {
            Instantiate(toolbarUIItemPrefab, itemsParent).Init(action.toolbarAction, action.editorAction);
        }
    }
}
