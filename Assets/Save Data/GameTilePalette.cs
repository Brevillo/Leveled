using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Leveled/GameTile Palette")]
public class GameTilePalette : ScriptableObject
{
    [SerializeField] private List<GameTile> tiles;

    public int GetID(TileBase tileBase) => tiles.FindIndex(tile => tile.TileBase == tileBase);
    public GameTile GetTile(int id) => tiles[id];

    public List<GameTile> Tiles => tiles;
}
