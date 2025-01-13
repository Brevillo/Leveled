using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Leveled/GameTile Editor State")]
public class TileEditorState : ScriptableObject
{
    [SerializeField] private GameTilePalette palette; 
    
    private GameTile activeTile;
    private ToolType activeTool;
    private GameState gameState;
    
    private Dictionary<Vector3Int, int> tiles;
    
    public void SetTile(Vector3Int position, GameTile tile)
    {
        if (tile == null)
        {
            tiles.Remove(position);
        }
        else
        {
            tiles[position] = palette.Tiles.IndexOf(tile);
        }
    }

    public GameTile GetTile(Vector3Int position) => tiles.TryGetValue(position, out int index)
        ? palette.Tiles[index] 
        : null;
    
    public void ResetTiles()
    {
        tiles = new();
    }
    public event Action<GameTile> ActiveTileChanged;
    public event Action<ToolType> ActiveToolChanged;
    public event Action<GameState> GameStateChanged;
    
    public GameTile ActiveTile
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

    public GameState GameState
    {
        get => gameState;
        set
        {
            gameState = value;
            GameStateChanged?.Invoke(value);
        }
    }

    public GameTilePalette Palette => palette;
    
    public Dictionary<Vector3Int, int> Tiles => tiles;

    public void LoadLevel(LevelData levelData)
    {
        LevelLoaded?.Invoke(levelData);
    }

    public event Action<LevelData> LevelLoaded;
}
