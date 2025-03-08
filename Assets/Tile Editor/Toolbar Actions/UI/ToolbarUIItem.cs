using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarUIItem : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image background;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color regularColor;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private TextMeshProUGUI keymap;
    [SerializeField] private UITooltip uiTooltip;
    
    private ToolbarAction tool;
    
    public void Init(ToolbarAction tool)
    {
        this.tool = tool;
        icon.sprite = tool.IconSprite;
        keymap.text = tool.Keymap;
        uiTooltip.Contents = tool.Tooltip;
    }

    public void Clicked()
    {
        tool.EditorAction.InvokeAction();
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
                
                background.color = toolbarChangeInfo.newValue == tool
                    ? selectedColor
                    : regularColor;
                
                break;
        }
    }
}
