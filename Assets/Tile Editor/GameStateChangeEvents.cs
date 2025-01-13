using System;
using UnityEngine;
using UnityEngine.Events;

public class GameStateChangeEvents : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private UnityEvent enteringPlayMode;
    [SerializeField] private UnityEvent exitingPlayMode;

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
            case GameState.Playing:
                enteringPlayMode.Invoke();
                break;
            
            case GameState.Editing:
                exitingPlayMode.Invoke();
                break;
        }
    }
}
