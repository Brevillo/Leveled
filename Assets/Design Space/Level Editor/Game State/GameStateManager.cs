using System;
using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Game State Manager")]
public class GameStateManager : ScriptableObject
{
    private EditorState editorState;

    public event Action<EditorState> EditorStateChanged;

    public void EnterPlayMode()
    {
        EditorState = EditorState.Playing;
    }

    public void EnterEditMode()
    {
        EditorState = EditorState.Editing;
    }

    public void StartTextInput()
    {
        EditorState = EditorState.TextInput;
    }
    
    public void ToggleGameState()
    {
        EditorState = EditorState == EditorState.Editing
            ? EditorState.Playing
            : EditorState.Editing;
    }

    public EditorState EditorState
    {
        get => editorState;
        set
        {
            if (editorState == value) return;

            SetEditorStateAndNotify(value);
        }
    }

    public void SetEditorStateAndNotify(EditorState value)
    {
        editorState = value;

        EditorStateChanged?.Invoke(value);
    }
}
