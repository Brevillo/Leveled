using System;
using TMPro;
using UnityEngine;

public class MetadataResolverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelDisplay;
    
    private MetadataResolver metadataResolver;

    public event Action<object> MetadataUpdated;
    public event Action<Vector2Int[]> Initialized;

    public MetadataResolver Resolver => metadataResolver;

    public void Initialize(MetadataResolver resolver, Vector2Int[] selection)
    {
        metadataResolver = resolver;
        labelDisplay.text = metadataResolver.FieldName;
        
        Initialized?.Invoke(selection);
    }
    
    public void UpdateMetadata(object metadata)
    {
        MetadataUpdated?.Invoke(metadataResolver.Transform(metadata));
    }
}