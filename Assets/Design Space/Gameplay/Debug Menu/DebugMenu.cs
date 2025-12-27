using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugMenu : MonoBehaviour
{
    [SerializeField] private DebugMenuButton buttonPrefab;
    [SerializeField] private float holdTime;
    [SerializeField] private InputActionReference toggleMenuInput;
    [SerializeField] private InputActionReference closeMenuInput;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private GameObject content;
    
    private TimeModifier timeModifier;

    private readonly List<DebugAction> debugActions = new()
    {
        new("Noclip", _ =>
        {
            var player = FindAnyObjectByType<PlayerMovement>();

            if (player == null) return;

            player.NoClip = !player.NoClip;
        }),
        
        new("Player Invincibility", _ =>
        {
            var player = FindAnyObjectByType<PlayerMovement>();

            if (player == null) return;

            var health = player.GetComponent<CharacterHealth>();
            health.Invincible = !health.Invincible;
        }),
        
        new("Half Time Scale", context => context.timeModifier.Multiplier /= 2f),
        new("Double Time Scale", context => context.timeModifier.Multiplier *= 2f),
        new("Reset Time Scale", context => context.timeModifier.Multiplier = 1f),
    };

    #region Input Binding
    
    private InputBinding[] inputBindings;

    private class InputBinding
    {
        private static readonly KeyCode[] invalidBindings = {
            KeyCode.Mouse0,
            KeyCode.Mouse1,
            KeyCode.BackQuote,
            KeyCode.Escape,
        };
        
        public static InputBinding[] CreateAll(DebugMenu debugMenu) => Enumerable
            .Range(0, Enum.GetNames(typeof(KeyCode)).Length)
            .Cast<KeyCode>()
            .Where(keyCode => !invalidBindings.Contains(keyCode))
            .Select(keyCode => new InputBinding(debugMenu, keyCode)).ToArray();
        
        private readonly DebugMenu debugMenu;
        private readonly KeyCode keyCode;
        private readonly string editorPrefKey; 
       
        private string action;
        
        private static readonly List<InputBinding> bindings = new();

        private InputBinding(DebugMenu debugMenu, KeyCode keyCode)
        {
            bindings.Add(this);
            
            this.debugMenu = debugMenu;
            this.keyCode = keyCode;
            
            editorPrefKey = $"{ProjectConstants.ProjectName}_DebugMenu_ActionBinding_{keyCode.ToString()}";

            action = "";
            
            BindAction(PlayerPrefs.GetString(editorPrefKey, ""));
        }

        private void BindAction(string selectedAction, bool forceExclusivity = false)
        {
            // Update display of current button
            UpdateButton("");

            // Remove binding from other actions
            if (forceExclusivity)
            {
                foreach (var binding in bindings
                             .Where(binding => binding != this && binding.action == selectedAction))
                {
                    binding.action = "";
                    
                    PlayerPrefs.DeleteKey(binding.editorPrefKey);
                }
            }
            
            // Set binding (toggle off if same as current binding)
            action = action == selectedAction 
                ? ""
                : selectedAction;
            
            PlayerPrefs.SetString(editorPrefKey, action);

            // Update display of new button
            UpdateButton(keyCode.ToString());

            void UpdateButton(string display)
            {
                if (debugMenu.actionButtons.TryGetValue(action, out var button))
                {
                    button.SetBindingDisplay(display);
                }
            }
        }

        public void Update()
        {
            if (!Input.GetKeyDown(keyCode)) return;

            // Bind Action
            if (debugMenu.activeBinding != default)
            {
                BindAction(debugMenu.activeBinding.action.name, forceExclusivity: true);
                debugMenu.EndBinding();
            }
            
            // Invoke Action
            else if (action != "" && debugMenu.actionsByName.ContainsKey(action))
            {
                debugMenu.actionsByName[action].action.Invoke(debugMenu);
            }
        }
    }

    private class DebugAction
    {
        public DebugAction(string name, Action<DebugMenu> action)
        {
            this.name = name;
            this.action = action;
        }

        public readonly string name;
        public readonly Action<DebugMenu> action;
    }

    #endregion
    
    #region UI

    private (DebugAction action, DebugMenuButton button) activeBinding;

    private Dictionary<string, DebugAction> actionsByName;
    
    private float holdTimer;
    private bool holding;

    private Dictionary<string, DebugMenuButton> actionButtons;

    private void EndBinding()
    {
        activeBinding = default;
    }
    
    private void Awake()
    {
        actionButtons = new();

        actionsByName = debugActions
            .ToDictionary(action => action.name);
        
        foreach (var debugAction in debugActions)
        {
            var button = Instantiate(buttonPrefab, buttonParent);
            button.SetNameDisplay(debugAction.name);

            var action = debugAction;
            button.ActionInvoked += () => action.action.Invoke(this); 
            button.BindingStarted += () =>
            {
                if (activeBinding.button != null && activeBinding.button != button)
                {
                    activeBinding.button.RevertBindingDisplay();
                }
                
                activeBinding = (debugAction, button);
            };
            
            actionButtons.Add(debugAction.name, button);
        }
        
        inputBindings = InputBinding.CreateAll(this);
        
        timeModifier = TimeModifier.New();
    }
    
    private void OnDestroy()
    {
        timeModifier.Destroy();
    }

    private bool Active
    {
        get => content.activeSelf;
        set
        {
            content.SetActive(value);

            if (!value)
            {
                activeBinding = default;
            }
        }
    }

    private void Update()
    {
        if (toggleMenuInput.action.WasPerformedThisFrame() || (Active && closeMenuInput.action.WasPerformedThisFrame()))
        {
            Active = !Active;

            holdTimer = 0f;
            holding = false;
        }

        holdTimer += Time.unscaledDeltaTime;

        bool pressing = toggleMenuInput.action.IsPressed();

        switch (pressing)
        {
            case true when holdTimer > holdTime:
                holding = true;
                break;
            case false when holding:
                Active = false;
                break;
        }

        foreach (var numberBinding in inputBindings)
        {
            numberBinding.Update();
        }
    }

    #endregion
}
