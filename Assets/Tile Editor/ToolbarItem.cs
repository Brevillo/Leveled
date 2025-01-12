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
    
    public void SetActiveTool()
    {
        editorState.ActiveTool = toolType;
    }

    private void Awake()
    {
        OnActiveToolChanged(editorState.ActiveTool);
    }

    private void OnEnable()
    {
        editorState.ActiveToolChanged += OnActiveToolChanged;
    }

    private void OnDisable()
    {
        editorState.ActiveToolChanged -= OnActiveToolChanged;
    }

    private void OnActiveToolChanged(ToolType activeTool)
    {
        background.color = activeTool == toolType
            ? selectedColor
            : regularColor;
    }
}
