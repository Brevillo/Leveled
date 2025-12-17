using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Prefab Tile")]
public class PrefabTile : TileBaseFactory
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Sprite previewTileSprite;
    [SerializeField] private TileGameObject prefabSpawnerPrefab;

    private TileGameObject spawnerInstance;
    
    protected override TileBase CreateTileBase()
    {
        var ruleTile = CreateInstance<RuleTile>();

        if (spawnerInstance != null)
        {
            Destroy(spawnerInstance.gameObject);
        }
        
        spawnerInstance = Instantiate(prefabSpawnerPrefab);
        spawnerInstance.Active = false;

        var spawner = spawnerInstance.GetComponentInChildren<PrefabSpawner>(true);
        spawner.prefab = prefab;
        spawner.PreviewSprite = previewTileSprite;
        
        ruleTile.m_DefaultGameObject = spawnerInstance.gameObject;
        ruleTile.m_DefaultColliderType = Tile.ColliderType.None;

        return ruleTile;
    }
}