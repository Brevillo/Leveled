using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = ProjectConstants.ProjectFolder + "Editor Action")]
public class EditorAction : ScriptableObject
{
    [SerializeField] private InputActionReference inputActionReference;
    [SerializeField] private EditorState validEditorStates = EditorState.Editing;
    [SerializeField] private string tooltip;
    [SerializeField] private Sprite icon;
    
    public event Action Action;

    private bool active;
    private bool enabled;

    public event Action<bool> EnabledChanged;
    
    public bool Enabled
    {
        get => enabled && active;
        set
        {
            if (active && enabled != value)
            {
                enabled = value;
                
                EnabledChanged?.Invoke(value);
            }
        }
    }
    
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
    
    public void Initialize(GameStateManager gameStateManager)
    {
        this.gameStateManager = gameStateManager;

        if (inputActionReference != null)
        {
            inputActionReference.action.performed += OnInputActionPerformed; 
        }

        active = true;
    }

    public void Cleanup()
    {
        if (inputActionReference != null)
        {
            inputActionReference.action.performed -= OnInputActionPerformed;
        }

        active = false;
    }
    
    private void OnInputActionPerformed(InputAction.CallbackContext context)
    {
        InvokeAction();
    }

    public void InvokeAction()
    {
        if (!validEditorStates.HasFlag(gameStateManager.EditorState)) return;
        
        Action?.Invoke();
    }
}
