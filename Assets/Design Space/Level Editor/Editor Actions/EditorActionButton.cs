using OliverBeebe.UnityUtilities.Runtime.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorActionButton : MonoBehaviour
{
    [SerializeField] private EditorAction editorAction;
    [Space]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI keymap;
    [SerializeField] private UITooltip uiTooltip;
    [SerializeField] private BoolSetting showKeymap;
    [SerializeField] private Button button;

    public void SetEditorAction(EditorAction editorAction)
    {
        this.editorAction = editorAction;
        
        if (editorAction == null) return;

        if (editorAction.IconSprite != null && icon != null)
        {
            icon.sprite = editorAction.IconSprite;
        }

        if (keymap != null)
        {
            keymap.text = editorAction.Keymap;
        }

        if (uiTooltip != null)
        {
            uiTooltip.Contents = editorAction.Tooltip;
        }
        
        if (button != null)
        {
            button.onClick.AddListener(editorAction.InvokeAction);
            editorAction.EnabledChanged += OnEditorActionEnabledChanged;
        }
    }

    private void OnEditorActionEnabledChanged(bool enabled)
    {
        if (button != null)
        {
            button.interactable = enabled;
        }
    }

    private void OnEnable()
    {
        SetEditorAction(editorAction);

        if (showKeymap != null)
        {
            showKeymap.ValueChanged += OnShowKeymapValueChanged;
            OnShowKeymapValueChanged(showKeymap.Value);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(editorAction.InvokeAction);
            editorAction.EnabledChanged -= OnEditorActionEnabledChanged;
        }

        if (showKeymap != null)
        {
            showKeymap.ValueChanged -= OnShowKeymapValueChanged;
        }
    }

    private void OnShowKeymapValueChanged(bool show)
    {
        keymap.enabled = show;
    }
}
