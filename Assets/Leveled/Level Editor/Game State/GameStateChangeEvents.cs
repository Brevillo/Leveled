using System;
using UnityEngine;
using UnityEngine.Events;

public class GameStateChangeEvents : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private UnityEvent enteringPlayMode;
    [SerializeField] private UnityEvent exitingPlayMode;
    [SerializeField] private UnityEvent<GameState> gameStateChanged;
    
    private void OnEnable()
    {
        gameStateManager.GameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        gameStateManager.GameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState gameState)
    {
        gameStateChanged.Invoke(gameState);
        
        switch (gameState)
        {
            case GameState.Playing:
                enteringPlayMode.Invoke();
                break;
            
            case GameState.Editing:
                exitingPlayMode.Invoke();
                break;
        }
    }
}
