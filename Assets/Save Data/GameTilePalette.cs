using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/GameTile Palette")]
public class GameTilePalette : ScriptableObject
{
    [SerializeField] private List<GameTile> tiles;

    public int GetIndex(GameTile tile) => tiles.IndexOf(tile);
    public GameTile GetTile(int index) => index != -1 ? tiles[index] : null;

    public List<GameTile> Tiles => tiles;
}
