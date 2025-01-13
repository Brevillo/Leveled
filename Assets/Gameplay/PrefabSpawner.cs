using System;
using UnityEngine;
using UnityEngine.Events;

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private UnityEvent enteringPlayMode;
    [SerializeField] private UnityEvent exitingPlayMode;
    
    private GameObject prefabInstance;
    
    private void OnEnable()
    {
        editorState.GameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        editorState.GameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Editing:
                
                if (prefabInstance != null)
                {
                    Destroy(prefabInstance);
                }
                
                exitingPlayMode.Invoke();
                
                break;
            
            case GameState.Playing:
                
                prefabInstance = Instantiate(prefab, transform);

                enteringPlayMode.Invoke();
                
                break;
        }
    }
}
