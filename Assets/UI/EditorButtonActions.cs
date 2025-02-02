using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.UI;

public class EditorButtonActions : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private List<Action> actions;
    
    [Serializable]
    private class Action
    {
        [SerializeField, HideInInspector] private string displayName;
        [SerializeField] private string name;
        [SerializeField] private Button button;
        [SerializeField] private GameState validGameStates = GameState.Editing;
        [SerializeField] private InputActionReference inputAction;
        [SerializeField] private UnityEvent action;

        private GameStateManager gameStateManager;

        public void UpdateDisplayName()
        {
            var values = Enum.GetValues(typeof(GameState)).Cast<GameState>().Where(value => validGameStates.HasFlag(value));
            
            displayName = $"{name} - {string.Join(", ", values)}";
        }
        
        public void Enable(GameStateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
            
            if (button != null)
            {
                button.onClick.AddListener(InvokeAction);
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
                button.onClick.RemoveListener(InvokeAction);
            }

            if (inputAction != null)
            {
                inputAction.action.performed -= OnInputActionPerformed;
            }
        }

        private void OnInputActionPerformed(InputAction.CallbackContext context)
        {
            InvokeAction();
        }

        private void InvokeAction()
        {
            if (!validGameStates.HasFlag(gameStateManager.GameState))
            {
                return;
            }
            
            action.Invoke();
        }
    }

    private void OnEnable()
    {
        foreach (var action in actions)
        {
            action.Enable(gameStateManager);
        }
    }

    private void OnDisable()
    {
        foreach (var action in actions)
        {
            action.Disable();
        }
    }

    private void OnValidate()
    {
        foreach (var action in actions)
        {
            action.UpdateDisplayName();
        }
    }
}
