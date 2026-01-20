using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignDataMetadataResolverUI : MonoBehaviour
{
    [SerializeField] private MetadataResolverUI metadataResolverUI;
    [SerializeField] private TMP_InputField textInputField;
    [SerializeField] private Toggle showInGameToggle;
    [SerializeField] private TileEditorState tileEditorState;
    [SerializeField] private TextMeshProUGUI placeholderTextDisplay;
    
    private SignData currentValue;

    private void Awake()
    {
        metadataResolverUI.Initialized += OnInitialized;
        
        textInputField.onEndEdit.AddListener(TextChanged);
        showInGameToggle.onValueChanged.AddListener(ShowInGameChanged);
    }

    private void OnInitialized(Vector2Int[] selection)
    {
        var resolver = (SignDataMetadataResolver)metadataResolverUI.Resolver;

        currentValue = resolver.GetCurrentValue<SignData>(selection, tileEditorState);
        
        textInputField.SetTextWithoutNotify(currentValue.text);
        showInGameToggle.SetIsOnWithoutNotify(currentValue.showInGame);

        placeholderTextDisplay.text = resolver.InputFieldPlaceholder;
    }

    private void ShowInGameChanged(bool showInGame)
    {
        currentValue = new(currentValue.text, showInGame);
        
        metadataResolverUI.UpdateMetadata(currentValue);
    }

    private void TextChanged(string text)
    {
        currentValue = new(text, currentValue.showInGame);
        
        metadataResolverUI.UpdateMetadata(currentValue);
    }
}
