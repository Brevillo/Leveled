using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ContentFolder + "Metadata Resolver")]
public class MetadataResolver : ScriptableObject
{
    [SerializeField] private string fieldName;
    [SerializeField] private MetadataResolverUI uiEntry;
    
    public string FieldName => fieldName;

    public MetadataResolverUI GetResolverUI(Transform parent) => Instantiate(uiEntry, parent);
}