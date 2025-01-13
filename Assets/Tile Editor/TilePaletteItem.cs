using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TilePaletteItem : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color regularColor;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private Image iconImage;
    [SerializeField] private UITooltip tooltip;

    private GameTile gameTile;
    
    public void Initialize(GameTile tile)
    {
        iconImage.sprite = tile.PaletteIcon;
        gameTile = tile;
        tooltip.Contents = tile.Tooltip;
        
        OnActiveTileChanged(editorState.ActiveTile);
    }
    
    public void SetActiveTile()
    {
        editorState.ActiveTile = gameTile;
    }

    private void OnEnable()
    {
        editorState.ActiveTileChanged += OnActiveTileChanged;
    }

    private void OnDisable()
    {
        editorState.ActiveTileChanged -= OnActiveTileChanged;
    }

    private void OnActiveTileChanged(GameTile activeTile)
    {
        background.color = activeTile == gameTile
            ? selectedColor
            : regularColor;

        if (editorState.ActiveTool != ToolType.Brush
            && editorState.ActiveTool != ToolType.RectBrush)
        {
            editorState.ActiveTool = ToolType.Brush;
        }
    }
}
