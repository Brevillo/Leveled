using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Leveled/GameTile Editor State")]
public class TileEditorState : ScriptableObject
{
    private GameTile activeTile = null;
    private ToolType activeTool = ToolType.Brush;
    
    private Dictionary<Vector3Int, GameTile> tiles = new();

    private Stack<ChangeInfo> undoLog = new();
    private Stack<ChangeInfo> redoLog = new();

    public Stack<ChangeInfo> UndoLog => undoLog;
    public Stack<ChangeInfo> RedoLog => redoLog;
    
    public event Action<ChangeInfo> EditorChanged;

    public void SendEditorChange(ChangeInfo changeInfo, bool logChange = true)
    {
        if (logChange)
        {
            undoLog.Push(changeInfo);
            redoLog.Clear();
        }
        
        EditorChanged?.Invoke(changeInfo);
        
        switch (changeInfo)
        {
            case ChangeInfoBundle editorChangeInfoBundle:

                foreach (var info in editorChangeInfoBundle.changeInfos)
                {
                    SendEditorChange(info, false);
                }
                
                break;
            
            case PaletteChangeInfo paletteChangeInfo:
                activeTile = paletteChangeInfo.newTile;
                break;
            
            case ToolbarChangeInfo toolbarChangeInfo:
                activeTool = toolbarChangeInfo.newTool;
                break;
            
            case TileChangeInfo tileChangeInfo:

                if (tileChangeInfo.newTile == null)
                {
                    tiles.Remove(tileChangeInfo.position);
                }
                else
                {
                    tiles[tileChangeInfo.position] = tileChangeInfo.newTile;
                }
                break;
            
            case MultiTileChangeInfo multiTileChangeInfo:

                for (int i = 0; i < multiTileChangeInfo.positions.Length; i++)
                {
                    var tile = multiTileChangeInfo.newTiles[i];
            
                    if (tile == null)
                    {
                        tiles.Remove(multiTileChangeInfo.positions[i]);
                    }
                    else
                    {
                        tiles[multiTileChangeInfo.positions[i]] = tile;
                    }
                }
                
                break;
        }
    }

    public void Undo()
    {
        if (!undoLog.TryPop(out var change)) return;
        
        var undoChange = change.Reverted;
        redoLog.Push(undoChange);
        SendEditorChange(undoChange, false);
    }

    public void Redo()
    {
        if (!redoLog.TryPop(out var change)) return;
        
        var redoChange = change.Reverted;
        undoLog.Push(redoChange);
        SendEditorChange(redoChange, false);
    }

    public void ClearChangelog()
    {
        undoLog.Clear();
        redoLog.Clear();
        EditorChanged?.Invoke(null);
    }
    
    #region State Modification
    
    public GameTile GetTile(Vector3Int position) => tiles.GetValueOrDefault(position);

    public void SetTile(Vector3Int position, GameTile tile)
    {
        var current = GetTile(position);

        if (tile == current)
        {
            return;
        }
        
        SendEditorChange(new TileChangeInfo(position, current, tile));
    }

    public void SetTiles(Vector3Int[] positions, GameTile[] tiles) =>
        SendEditorChange(GetMultiTileChangeInfo(positions, tiles));

    public ChangeInfo GetMultiTileChangeInfo(Vector3Int[] positions, GameTile[] tiles) =>
        new MultiTileChangeInfo(positions, positions.Select(GetTile).ToArray(), tiles);
    
    public GameTile ActiveTile
    {
        get => activeTile;
        set => SendEditorChange(new PaletteChangeInfo(activeTile, value), false);
    }

    public ToolType ActiveTool
    {
        get => activeTool;
        set => SendEditorChange(new ToolbarChangeInfo(activeTool, value), false);
    }
    
    #endregion
    
    #region Get/Set Level Data
    
    public LevelData GetLevelData(GameTilePalette palette) => new()
    {
        positions = tiles.Keys.ToArray(),
        ids = tiles.Values.Select(palette.GetID).ToArray(),
    };

    public void ClearLevelData()
    {
        // Clear Tiles
        var nullTiles = new GameTile[tiles.Count];
        Array.Fill(nullTiles, null);
        
        SetTiles(tiles.Keys.ToArray(), nullTiles);
        
        // Reset editor state
        ClearChangelog();
        tiles.Clear();
    }
    
    public void SetLevelData(LevelData levelData, GameTilePalette palette)
    {
        // Clear changelog
        ClearChangelog();

        // Clear current level, then fill in new level
        var nullTiles = new GameTile[tiles.Count];
        Array.Fill(nullTiles, null);
        
        SendEditorChange(new ChangeInfoBundle($"Loaded Level", new ChangeInfo[]
        {
            new MultiTileChangeInfo(tiles.Keys.ToArray(), tiles.Keys.Select(GetTile).ToArray(), nullTiles),
            new MultiTileChangeInfo(levelData.positions, levelData.positions.Select(GetTile).ToArray(), levelData.ids.Select(palette.GetTile).ToArray()),
        }), false);
    }
    
    #endregion
}
