using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EditorActionButton : MonoBehaviour
{
    [SerializeField] private EditorAction editorAction;
    [Space]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI keymap;
    [SerializeField] private UITooltip uiTooltip;
    
    private Button button;

    public void SetEditorAction(EditorAction editorAction)
    {
        this.editorAction = editorAction;
        
        if (editorAction == null) return;

        if (editorAction.IconSprite != null) icon.sprite = editorAction.IconSprite;
        if (keymap != null) keymap.text = editorAction.Keymap;
        if (uiTooltip != null) uiTooltip.Contents = editorAction.Tooltip;
        
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(editorAction.InvokeAction);
        }
    }

    private void OnEnable()
    {
        SetEditorAction(editorAction);
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(editorAction.InvokeAction);
        }
    }
}
