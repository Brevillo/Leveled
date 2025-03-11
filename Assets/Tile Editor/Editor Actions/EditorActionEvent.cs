using System;
using UnityEngine;
using UnityEngine.Events;

public class EditorActionEvent : MonoBehaviour
{
    [SerializeField] private EditorAction editorAction;
    [Space]
    [SerializeField] private UnityEvent actionEvent;
    public event Action ActionEvent;

    public EditorAction EditorAction
    {
        get => editorAction;
        set
        {
            if (enabled && editorAction != null)
            {
                editorAction.Action -= OnAction;
            }

            editorAction = value;

            if (enabled && editorAction != null)
            {
                editorAction.Action += OnAction;
            }
        }
    }

    private void OnEnable()
    {
        editorAction.Action += OnAction;
    }

    private void OnDisable()
    {
        editorAction.Action -= OnAction;
    }

    private void OnAction()
    {
        ActionEvent?.Invoke();
        actionEvent.Invoke();
    }
}
