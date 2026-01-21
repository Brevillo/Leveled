using System;
using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ContentFolder + "Metadata Viewport Data")]
public class MetadataViewportData : ScriptableObject
{
    [SerializeField] private ChangeloggedBool showMetadataVisuals;
    [SerializeField] private MetadataViewportVisual metadataViewportVisualPrefab;
    [SerializeField] private string metadataType;
    
    public Type MetadataType => Type.GetType(metadataType); 

    public bool ShowVisuals => showMetadataVisuals.Value;

    public MetadataViewportVisual MetadataViewportVisualPrefab => metadataViewportVisualPrefab;
}
