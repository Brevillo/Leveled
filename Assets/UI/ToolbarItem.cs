using System;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarItem : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color regularColor;
    [SerializeField] private ToolType toolType;
    [SerializeField] private TileEditorState editorState;

    private void OnEnable()
    {
        editorState.EditorChanged += OnEditorChanged;
    }

    private void OnDisable()
    {
        editorState.EditorChanged -= OnEditorChanged;
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case ToolbarChangeInfo toolbarChangeInfo:
                
                background.color = toolbarChangeInfo.newTool == toolType
                    ? selectedColor
                    : regularColor;
                
                break;
        }
    }
}
