using UnityEngine;

public abstract class MetadataResolver : ScriptableObject
{
    protected const string CreateAssetMenuPath = ProjectConstants.ContentFolder + "Metadata Resolvers/";
    
    [SerializeField] private string fieldName;
    [SerializeField] private MetadataResolverUI uiEntry;
    [SerializeField] private string inputFieldPlaceholder;

    public abstract object Transform(string metadata);
    public abstract string[] GetOptions(TileEditorState tileEditorState);
    public abstract string GetCurrentValue(Vector2Int[] selection, TileEditorState tileEditorState);

    public string FieldName => fieldName;

    public string InputFieldPlaceholder => inputFieldPlaceholder;

    public MetadataResolverUI GetResolverUI(Transform parent) => Instantiate(uiEntry, parent);
}