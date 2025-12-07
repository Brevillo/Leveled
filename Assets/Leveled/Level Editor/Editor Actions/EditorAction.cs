using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = ProjectConstants.ProjectFolder + "Editor Action")]
public class EditorAction : ScriptableObject
{
    [SerializeField] private InputActionReference inputActionReference;
    [SerializeField] private GameState validGameStates = GameState.Editing;
    [SerializeField] private string tooltip;
    [SerializeField] private Sprite icon;
    
    public event Action Action;

    public string Keymap
    {
        get
        {
            if (inputActionReference == null) return "";

            string bindingDisplay = inputActionReference.action.bindings[0].ToDisplayString();

            if (bindingDisplay.Length > 1) return "";

            return bindingDisplay;
        }
    }

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
    }
}
