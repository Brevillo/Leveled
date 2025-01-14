using UnityEngine;
using UnityEngine.UI;

public class GameStateToggle : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite editIcon;
    [SerializeField] private GameStateManager gameStateManager;

    public void ToggleGameState()
    {
        var newGameState = gameStateManager.GameState == GameState.Editing
            ? GameState.Playing
            : GameState.Editing;

        gameStateManager.GameState = newGameState;

        icon.sprite = newGameState == GameState.Playing
            ? editIcon
            : playIcon;
    }
}
