using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EditorButtonActions : MonoBehaviour
{
    [SerializeField] private List<Action> actions;
    
    [Serializable]
    private class Action
    {
        public string name;
        public Button button;
        public InputActionReference inputAction;
        public UnityEvent action;

        public void Enable()
        {
            if (button != null)
            {
                button.onClick.AddListener(action.Invoke);
            }

            if (inputAction != null)
            {
                inputAction.action.performed += OnInputActionPerformed;
            }
        }

        public void Disable()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action.Invoke);
            }

            if (inputAction != null)
            {
                inputAction.action.performed -= OnInputActionPerformed;
            }
        }

        private void OnInputActionPerformed(InputAction.CallbackContext context)
        {
            action.Invoke();
        }
    }

    private void OnEnable()
    {
        foreach (var action in actions)
        {
            action.Enable();
        }
    }

    private void OnDisable()
    {
        foreach (var action in actions)
        {
            action.Disable();
        }
    }
}
