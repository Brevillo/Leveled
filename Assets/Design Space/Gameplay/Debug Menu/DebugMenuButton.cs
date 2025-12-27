using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenuButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberBinding;
    [SerializeField] private TextMeshProUGUI nameDisplay;
    [SerializeField] private Image background;
    [SerializeField] private Color bindingColor;
    [SerializeField] private Color defaultColor;
    [SerializeField] private string noBindingText;
    [SerializeField] private string bindingText;
    
    public event Action BindingStarted;
    public event Action ActionInvoked;

    private bool isBinding;
    private string revertBinding;

    private void OnDisable()
    {
        background.color = defaultColor;
    }

    public void OnClick()
    {
        ActionInvoked?.Invoke();
    }

    public void StartBinding()
    {
        if (isBinding)
        {
            SetBindingDisplay("");
            return;
        }

        revertBinding = numberBinding.text;
        isBinding = true;
        
        background.color = bindingColor;
        numberBinding.text = bindingText;

        BindingStarted?.Invoke();
    }
    
    public void SetNameDisplay(string name)
    {
        nameDisplay.text = name;
    }

    public void RevertBindingDisplay()
    {
        SetBindingDisplay(revertBinding);
    }
    
    public void SetBindingDisplay(string binding)
    {
        isBinding = false;
        numberBinding.text = binding == "" ? noBindingText : binding;
        background.color = defaultColor;
    }
}
