using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = ProjectConstants.ContentFolder + "GameTile")]
public class GameTile : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private Sprite paletteIcon;
    [SerializeField] private TileBase tileBase;
    [SerializeField] private GameObject entity;
    [SerializeField] private bool globalEntity;
    [SerializeField] private List<Tilemap> tilemapPrefabs;
    [SerializeField] private string tooltip;
    [SerializeField] private bool linkable;

    public int ID => id;
    public Sprite PaletteIcon => paletteIcon;
    public List<Tilemap> TilemapPrefabs => tilemapPrefabs;
    public string Tooltip => tooltip;
    public bool Linkable => linkable;

    public bool IsNullTileBase => tileBase == null;

    public TileBase TileBase => tileBase;

    public GameObject Entity => entity;

    public bool HasEntity => entity != null;

    public bool GlobalEntity => globalEntity;

    public static string NullableToString(GameTile tile) => tile != null
        ? tile.Tooltip
        : "null";
}