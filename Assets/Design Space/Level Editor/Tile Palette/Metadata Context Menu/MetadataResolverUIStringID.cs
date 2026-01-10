using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetadataResolverUIStringID : MonoBehaviour
{
    [SerializeField] private GameObject popup;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button optionPrefab;
    [SerializeField] private Transform optionsParent;
    [SerializeField] private TextMeshProUGUI currentValueDisplay;
    [SerializeField] private TextMeshProUGUI previousValueDisplay;
    [SerializeField] private TextMeshProUGUI placeholderTextDisplay;
    
    private List<Button> optionInstances;
    
    public event Action<string> OptionSelected;
    
    public void Initialize(string selected, string inputFieldPlaceholder, string[] options)
    {
        currentValueDisplay.text = selected;
        placeholderTextDisplay.text = inputFieldPlaceholder;
        
        foreach (var option in options)
        {
            var newOption = Instantiate(optionPrefab, optionsParent);

            newOption.GetComponentInChildren<TextMeshProUGUI>().text = option;
            newOption.onClick.AddListener(() => SelectOption(option));
            
            optionInstances.Add(newOption);
        }
    }
    
    private void Awake()
    {
        optionInstances = new();
        
        inputField.onSubmit.AddListener(SelectOption);
    }

    private void SelectOption(string value)
    {
        currentValueDisplay.text = value;

        OptionSelected?.Invoke(value);

        CloseValueEditor();
    }

    private void Start()
    {
        popup.SetActive(false);
    }

    public void OpenValueEditor()
    {
        previousValueDisplay.text = currentValueDisplay.text;
        
        popup.SetActive(true);
    }

    public void CloseValueEditor()
    {
        popup.SetActive(false);
    }
}