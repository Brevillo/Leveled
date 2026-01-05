using UnityEngine;
using UnityEngine.UI;

public class ToolbarUIItem : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color regularColor;
    [SerializeField] private EditorActionButton editorActionButton;
    [SerializeField] private ChangeloggedToolbarAction activeTool;
    
    private ToolbarAction toolbarAction;
    
    public void Init(ToolbarAction toolbarAction, EditorAction editorAction)
    {
        this.toolbarAction = toolbarAction;
        editorActionButton.SetEditorAction(editorAction);
    }
    
    private void OnEnable()
    {
        activeTool.ChangeEvent += OnChangeEvent;
    }

    private void OnDisable()
    {
        activeTool.ChangeEvent -= OnChangeEvent;
    }

    private void OnChangeEvent(ChangeInfo changeInfo)
    {
        background.color = activeTool.Value == toolbarAction
            ? selectedColor
            : regularColor;
    }
}
