using UnityEngine;
using UnityEngine.UI;

public class GameStateToggle : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite editIcon;
    [SerializeField] private TileEditorState editorState;

    public void ToggleGameState()
    {
        var newGameState = editorState.GameState == GameState.Editing
            ? GameState.Playing
            : GameState.Editing;

        editorState.GameState = newGameState;

        icon.sprite = newGameState == GameState.Playing
            ? editIcon
            : playIcon;
    }
}
