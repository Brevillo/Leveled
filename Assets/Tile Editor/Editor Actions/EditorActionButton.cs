using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EditorActionButton : MonoBehaviour
{
    [SerializeField] private EditorAction editorAction;
    
    private Button button;

    private void OnEnable()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(editorAction.InvokeAction);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(editorAction.InvokeAction);
        }
    }
}
