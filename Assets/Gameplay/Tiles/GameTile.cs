using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Leveled/Editor Content/GameTile", order = 100)]
public class GameTile : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private Sprite paletteIcon;
    [SerializeField] private TileBase tileBase;
    [SerializeField] private Tilemap tilemapPrefab;
    [SerializeField] private string tooltip;
    [SerializeField] private bool linkable;

    public int ID => id;
    public Sprite PaletteIcon => paletteIcon;
    public TileBase TileBase => tileBase;
    public Tilemap TilemapPrefab => tilemapPrefab;
    public string Tooltip => tooltip;
    public bool Linkable => linkable;
    
    public static string NullableToString(GameTile tile) => tile != null
        ? tile.Tooltip 
        : "null";
}