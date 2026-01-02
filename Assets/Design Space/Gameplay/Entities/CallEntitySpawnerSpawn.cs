using UnityEngine;

public class CallEntitySpawnerSpawn : MonoBehaviour
{
    [SerializeField] private EntitySpawnerReference entitySpawner;
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private Transform spawnPosition;

    public void Spawn()
    {
        entitySpawner.value.SpawnEntity(entityPrefab, spawnPosition.position);
    }
}
