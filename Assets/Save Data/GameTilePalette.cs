using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/GameTile Palette")]
public class GameTilePalette : ScriptableObject
{
    [SerializeField] private List<GameTile> tiles;

    public int GetID(GameTile tile) => tiles.IndexOf(tile);
    public GameTile GetTile(int id) => id != -1 ? tiles[id] : null;

    public List<GameTile> Tiles => tiles;
}
