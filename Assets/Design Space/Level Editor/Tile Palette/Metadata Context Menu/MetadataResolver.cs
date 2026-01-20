using UnityEngine;

public abstract class MetadataResolver : ScriptableObject
{
    protected const string CreateAssetMenuPath = ProjectConstants.ContentFolder + "Metadata Resolvers/";
    
    [SerializeField] private string fieldName;
    [SerializeField] private MetadataResolverUI uiEntry;
    [SerializeField] private string inputFieldPlaceholder;

    public virtual object Transform(object metadata) => metadata;
    public abstract object GetCurrentValue(Vector2Int[] selection, TileEditorState tileEditorState);

    public T GetCurrentValue<T>(Vector2Int[] selection, TileEditorState tileEditorState) =>
        (T)GetCurrentValue(selection, tileEditorState);
    
    public string FieldName => fieldName;

    public string InputFieldPlaceholder => inputFieldPlaceholder;

    public MetadataResolverUI GetResolverUI(Transform parent) => Instantiate(uiEntry, parent);
}

public abstract class StringMetadataResolver : MetadataResolver
{
    public abstract string[] GetOptions(TileEditorState tileEditorState);
}