using System;
using TMPro;
using UnityEngine;

public class MetadataResolverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelDisplay;
    
    public event Action<object> MetadataUpdated;
    public event Action<Vector2Int[]> Initialized;
    
    public void Initialize(MetadataResolver resolver, Vector2Int[] selection)
    {
        labelDisplay.text = resolver.FieldName;
        
        Initialized?.Invoke(selection);
    }
    
    public void UpdateMetadata(object metadata)
    {
        MetadataUpdated?.Invoke(metadata);
    }
}