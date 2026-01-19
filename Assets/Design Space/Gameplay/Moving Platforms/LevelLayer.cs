using UnityEngine;

public class LevelLayer : MonoBehaviour
{
    [SerializeField] private TileEditorState tileEditorState;
    
    private int layerID;

    public Metadata Metadata => tileEditorState.LevelInstance.GetLayerMetadata(layerID);
    
    public void Initialize(int layerID)
    {
        this.layerID = layerID;
    }
}