using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Leveled/Tile Editor State")]
public class TileEditorState : ScriptableObject
{
    private TileBase activeTile;
    private ToolType activeTool;

    public event Action<TileBase> ActiveTileChanged;
    public event Action<ToolType> ActiveToolChanged;
    
    public TileBase ActiveTile
    {
        get => activeTile;
        set
        {
            activeTile = value;
            ActiveTileChanged?.Invoke(value);
        }
    }

    public ToolType ActiveTool
    {
        get => activeTool;
        set
        {
            activeTool = value;
            ActiveToolChanged?.Invoke(value);
        }
    }
}
