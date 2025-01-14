using System;
using UnityEngine;
using UnityEngine.Events;

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    
    private GameObject prefabInstance;
    
    public void OnGameStateChanged(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Editing:
                
                if (prefabInstance != null)
                {
                    Destroy(prefabInstance);
                }
                
                break;
            
            case GameState.Playing:
                
                prefabInstance = Instantiate(prefab, transform);
                
                break;
        }
    }
}
