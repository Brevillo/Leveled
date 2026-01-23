using System;
using UnityEngine;

public class MetadataResolverUIEntity : MonoBehaviour
{
    [SerializeField] private MetadataResolverUI metadataResolverUI;
    [SerializeField] private GameObject popup;
    [SerializeField] private MetadataResolverEntityOption optionPrefab;
    [SerializeField] private Transform optionsParent;
    [SerializeField] private MetadataResolverEntityOption previousValueDisplay;
    [SerializeField] private MetadataResolverEntityOption currentValueDisplay;
    [SerializeField] private TileEditorState tileEditorState;
    [SerializeField] private GameTilePalette gameTilePalette;
    
    private void Awake()
    {
        metadataResolverUI.Initialized += OnInitialized;
    }

    private void Start()
    {
        popup.SetActive(false);

        var popupTransform = (RectTransform)popup.transform;
        var parent = SpaceUtility.GetCanvas(popupTransform).transform;
        SpaceUtility.ReParentPreserveRect(popupTransform, parent);
    }

    private void OnDestroy()
    {
        Destroy(popup);
    }

    private void OnInitialized(Vector2Int[] selection)
    {
        var resolver = (EntityMetadataResolver)metadataResolverUI.Resolver;

        currentValueDisplay.GameTile =
            gameTilePalette.GetTile(resolver.GetCurrentValue<int>(selection, tileEditorState));
        
        foreach (var option in resolver.GetOptions())
        {
            var newOption = Instantiate(optionPrefab, optionsParent);

            newOption.GameTile = option;
            newOption.OnClick.AddListener(() => SelectOption(option));
        }
    }

    private void SelectOption(GameTile value)
    {
        currentValueDisplay.GameTile = value;
        
        metadataResolverUI.UpdateMetadata(new GameTileID(value.ID));
        
        CloseValueEditor();
    }

    public void OpenValueEditor()
    {
        previousValueDisplay.GameTile = currentValueDisplay.GameTile;
        
        popup.SetActive(true);
    }

    public void CloseValueEditor()
    {
        popup.SetActive(false);
    }
}