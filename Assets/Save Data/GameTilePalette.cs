using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/GameTile Palette")]
public class GameTilePalette : ScriptableObject
{
    [SerializeField] private List<GameTile> tiles;

    public GameTile GetTile(int id) => tiles.FirstOrDefault(tile => tile.ID == id);

    public List<GameTile> Tiles => tiles;
}
