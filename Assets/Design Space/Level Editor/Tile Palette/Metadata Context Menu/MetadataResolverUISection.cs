using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MetadataResolverUISection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameDisplay;
    [SerializeField] private Transform fieldsParent;
    [SerializeField] private TileEditorState tileEditorState;
    
    private Vector2Int[] selection;
    
    public void Initialize(string name, List<MetadataResolver> resolvers, Vector2Int[] selection)
    {
        this.selection = selection;
        nameDisplay.text = name;

        foreach (var resolver in resolvers)
        {
            var resolverUI = resolver.GetResolverUI(fieldsParent);
            
            resolverUI.Initialize(resolver, selection);
            
            resolverUI.MetadataUpdated += OnMetadataUpdated;
        }
    }

    private void OnMetadataUpdated(object metadata)
    {
        tileEditorState.SetTiles(
            selection,
            selection
                .Select(position => tileEditorState.Level.GetTile(position).SetMetaData(metadata))
                .ToArray(),
            "Updated tile metadata");
    }
}