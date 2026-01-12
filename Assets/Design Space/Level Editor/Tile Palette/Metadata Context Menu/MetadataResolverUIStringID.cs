using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetadataResolverUIStringID : MonoBehaviour
{
    [SerializeField] private MetadataResolverUI metadataResolverUI;
    [SerializeField] private GameObject popup;
    [SerializeField] private GameObject inputFieldContent;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button optionPrefab;
    [SerializeField] private Transform optionsParent;
    [SerializeField] private TextMeshProUGUI currentValueDisplay;
    [SerializeField] private TextMeshProUGUI previousValueDisplay;
    [SerializeField] private TextMeshProUGUI placeholderTextDisplay;
    [SerializeField] private TileEditorState tileEditorState;
    
    public event Action<string> OptionSelected;
    
    private void Awake()
    {
        inputField.onSubmit.AddListener(SelectOption);
        
        metadataResolverUI.Initialized += Initialize;
    }
    
    public void Initialize(Vector2Int[] selection)
    {
        var resolver = metadataResolverUI.Resolver;

        currentValueDisplay.text =
            resolver.GetCurrentValue(selection, tileEditorState);
        
        placeholderTextDisplay.text = resolver.InputFieldPlaceholder;
        inputFieldContent.SetActive(placeholderTextDisplay.text != "");
        
        foreach (var option in resolver.GetOptions(tileEditorState))
        {
            var newOption = Instantiate(optionPrefab, optionsParent);

            newOption.GetComponentInChildren<TextMeshProUGUI>().text = option;
            newOption.onClick.AddListener(() => SelectOption(option));
        }
    }
    
    private void SelectOption(string value)
    {
        currentValueDisplay.text = value;

        metadataResolverUI.UpdateMetadata(value);

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