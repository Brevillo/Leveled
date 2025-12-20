using System;
using UnityEngine;
using UnityEngine.Events;

public class GameStateChangeEvents : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private UnityEvent enteringPlayMode;
    [SerializeField] private UnityEvent exitingPlayMode;
    [SerializeField] private UnityEvent<EditorState> gameStateChanged;
    
    private void OnEnable()
    {
        gameStateManager.GameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        gameStateManager.GameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(EditorState editorState)
    {
        gameStateChanged.Invoke(editorState);
        
        switch (editorState)
        {
            case EditorState.Playing:
                enteringPlayMode.Invoke();
                break;
            
            case EditorState.Editing:
                exitingPlayMode.Invoke();
                break;
        }
    }
}
