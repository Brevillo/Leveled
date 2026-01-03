using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ContentFolder + "Game Tile Category")]
public class GameTileCategory : ScriptableObject
{
    [SerializeField] private string displayTitle;
    [SerializeField] private List<GameTile> gameTiles;

    public List<GameTile> GameTiles => gameTiles;
    public string DisplayTitle => displayTitle;
}
