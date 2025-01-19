using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Game State Manager")]
public class GameStateManager : ScriptableObject
{
    private GameState gameState;

    public event Action<GameState> GameStateChanged;

    public void ToggleGameState()
    {
        GameState = GameState == GameState.Editing
            ? GameState.Playing
            : GameState.Editing;
    }
    
    public GameState GameState
    {
        get => gameState;
        set
        {
            gameState = value;
            GameStateChanged?.Invoke(value);
        }
    }
}
