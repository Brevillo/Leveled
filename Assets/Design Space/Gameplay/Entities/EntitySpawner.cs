using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class EntitySpawner : MonoBehaviour
{
    [SerializeField] private TileEditorState tileEditorState;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private PerspectiveCameraPositioning cameraPositioning;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private Vector2 spawnBuffer;
    
    private List<Entity> managedEntities;
    private List<GameObject> activeEntities;

    private class Entity
    {
        public readonly Vector2 position;
        private readonly TileData tileData;
        private readonly int layerID;
        private readonly EntitySpawner spawner;

        private bool active;
        private GameObject instance;
        
        public Entity(Vector2 position, TileData tileData, int layerID, EntitySpawner spawner)
        {
            this.position = position;
            this.tileData = tileData;
            this.layerID = layerID;
            this.spawner = spawner;

            if (tileData.gameTile.GlobalEntity)
            {
                SpawnEntity();
            }
        }
        
        public void Update(bool spawn)
        {
            if (active && instance == null || tileData.gameTile.GlobalEntity) return;
            
            if (spawn && !active)
            {
                SpawnEntity();
                active = true;
            }
            else if (!spawn && active)
            {
                spawner.DestroyEntity(instance);
                active = false;
            }
        }

        private void SpawnEntity()
        {
            var rotation = Quaternion.AngleAxis(tileData.GetMetaData<EnumStruct<TileOrientation>>().value switch
            {
                TileOrientation.Down => 180f,
                TileOrientation.Left => 90f,
                TileOrientation.Right => -90f,
                _ => 0f,
            }, Vector3.forward);
            
            instance = spawner.SpawnEntity(tileData.gameTile.Entity, position, rotation, layerID);

            if (instance.TryGetComponent(out TileEntity tileEntity))
            {
                Vector2Int cellPosition = spawner.spaceUtility.WorldToCell(position);
                tileEntity.Initialize(layerID, cellPosition,
                    spawner.tileEditorState.LevelInstance.GetTile(cellPosition, layerID).metadata);
            }
        }
    }

    public GameObject SpawnEntity(GameObject entityPrefab, Vector2 position, Quaternion rotation, int layerID = 0)
    {
        var entityInstance = Instantiate(entityPrefab, position, rotation, transform);
        
        activeEntities.Add(entityInstance);
        
        return entityInstance;
    }

    public void DestroyEntity(GameObject entityInstance)
    {
        activeEntities.Remove(entityInstance);
        
        Destroy(entityInstance);
    }

    private void Awake()
    {
        managedEntities = new();
        activeEntities = new();
        
        gameStateManager.EditorStateChanged += OnEditorStateChanged;
    }

    private void OnDestroy()
    {
        gameStateManager.EditorStateChanged -= OnEditorStateChanged;
    }
    
    private void OnEditorStateChanged(EditorState editorState)
    {
        switch (editorState)
        {
            case EditorState.Playing:

                foreach (var layer in tileEditorState.LevelInstance.AllLayerIDs)
                {
                    managedEntities.AddRange(tileEditorState.LevelInstance.GetLayerEntityPositions(layer)
                        .Select(tile => new Entity(spaceUtility.CellToWorld(tile), tileEditorState.LevelInstance.GetTileOnAnyLayer(tile), layer, this)));
                }

                break;
            
            default:

                foreach (var entity in activeEntities.ToArray())
                {
                    DestroyEntity(entity);
                }
                
                managedEntities.Clear();
                
                break;
        }
    }

    private void Update()
    {
        var cameraBounds = cameraPositioning.CameraRect;
        var spawnBounds = new Rect
        {
            size = cameraBounds.size + Vector2.one * spawnBuffer * 2f,
            center = cameraBounds.center,
        };
        
        foreach (var entity in managedEntities)
        {
            entity.Update(spawnBounds.Contains(entity.position));
        }
    }
}
