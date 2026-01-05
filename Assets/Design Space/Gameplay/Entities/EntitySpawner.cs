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
        private readonly EntitySpawner spawner;

        private bool active;
        private GameObject instance;
        
        public Entity(Vector2 position, TileData tileData, EntitySpawner spawner)
        {
            this.position = position;
            this.tileData = tileData;
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
            instance = spawner.SpawnEntity(tileData.gameTile.Entity, position);
        }
    }

    public GameObject SpawnEntity(GameObject entityPrefab, Vector2 position)
    {
        var entityInstance = Instantiate(entityPrefab, position, Quaternion.identity, transform);
        
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

                managedEntities = tileEditorState.Level.EntityTiles
                    .Select(kv => new Entity(spaceUtility.CellToWorld(kv.Key), kv.Value, this))
                    .ToList();
                
                break;
            
            default:
                
                managedEntities.Clear();

                foreach (var entity in activeEntities.ToArray())
                {
                    DestroyEntity(entity);
                }
                
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
