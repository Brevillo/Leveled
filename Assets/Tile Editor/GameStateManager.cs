using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Game State Manager")]
public class GameStateManager : ScriptableObject
{
    private GameState gameState;

    public event Action<GameState> GameStateChanged;

    public void EnterPlayMode()
    {
        GameState = GameState.Playing;
    }

    public void ExitPlayMode()
    {
        GameState = GameState.Editing;
    }
    
    public void ToggleGameState()
    {
        GameState = GameState == GameState.Editing
            ? GameState.Playing
            : GameState.Editing;
    }

    public void SetGameStateWithNotify(GameState gameState)
    {
        this.gameState = gameState;
        
        GameStateChanged?.Invoke(gameState);
    }
    
    public GameState GameState
    {
        get => gameState;
        set
        {
            if (gameState != value)
            {
                SetGameStateWithNotify(value);
            }
        }
    }
}
