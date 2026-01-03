using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ContentFolder + "GameTile Palette")]
public class GameTilePalette : ScriptableObject
{
    [SerializeField] private List<GameTileCategory> categories;

    public GameTile GetTile(int id) => categories
        .SelectMany(category => category.GameTiles)
        .FirstOrDefault(tile => tile.ID == id);
    
    public List<GameTileCategory> Categories => categories;
}
