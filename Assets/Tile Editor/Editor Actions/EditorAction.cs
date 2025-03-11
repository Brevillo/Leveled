using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Leveled/Editor Actions/Editor Action", order = 100)]
public class EditorAction : ScriptableObject
{
    [SerializeField] private InputActionReference inputActionReference;
    [SerializeField] private GameState validGameStates = GameState.Editing;
    [SerializeField] private string tooltip;
    [SerializeField] private Sprite icon;
    
    [SerializeField] private UnityEvent action;
    public event Action Action;

    public string Keymap =>
        inputActionReference != null ? inputActionReference.action.bindings[0].ToDisplayString() : "";
    public Sprite IconSprite => icon;
    public string Tooltip => tooltip;
    
    private GameStateManager gameStateManager;
    
    public void Enable(GameStateManager gameStateManager)
    {
        this.gameStateManager = gameStateManager;

        if (inputActionReference != null)
        {
            inputActionReference.action.performed += OnInputActionPerformed; 
        }
    }

    public void Disable()
    {
        if (inputActionReference != null)
        {
            inputActionReference.action.performed -= OnInputActionPerformed;
        }
    }
    
    private void OnInputActionPerformed(InputAction.CallbackContext context)
    {
        InvokeAction();
    }

    public void InvokeAction()
    {
        if (!validGameStates.HasFlag(gameStateManager.GameState)) return;
        
        Action?.Invoke();
        action.Invoke();
    }
}
