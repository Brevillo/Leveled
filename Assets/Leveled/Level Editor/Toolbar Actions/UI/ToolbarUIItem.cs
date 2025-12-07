using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarUIItem : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color regularColor;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private EditorActionButton editorActionButton;

    private ToolbarAction toolbarAction;
    
    public void Init(ToolbarAction toolbarAction, EditorAction editorAction)
    {
        this.toolbarAction = toolbarAction;
        editorActionButton.SetEditorAction(editorAction);
    }
    
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
                
                background.color = toolbarChangeInfo.newValue == toolbarAction
                    ? selectedColor
                    : regularColor;
                
                break;
        }
    }
}
