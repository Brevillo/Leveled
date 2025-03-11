using System;
using System.Collections.Generic;
using UnityEngine;

public class ToolbarUIManager : MonoBehaviour
{
    [Serializable]
    private struct ToolbarEditorAction
    {
        public ToolbarAction toolbarAction;
        public EditorAction editorAction;
    }
    
    [SerializeField] private List<ToolbarEditorAction> actions;
    [SerializeField] private ToolbarUIItem toolbarUIItemPrefab;
    [SerializeField] private Transform itemsParent;

    private void Awake()
    {
        foreach (var action in actions)
        {
            Instantiate(toolbarUIItemPrefab, itemsParent).Init(action.toolbarAction, action.editorAction);
        }
    }
}
