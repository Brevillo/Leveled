using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TilePaletteItem : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color regularColor;
    [SerializeField] private TileBase tile;
    [SerializeField] private TileEditorState editorState;
    
    public void SetActiveTile()
    {
        editorState.ActiveTile = tile;
    }

    private void OnEnable()
    {
        editorState.ActiveTileChanged += OnActiveTileChanged;
    }

    private void OnDisable()
    {
        editorState.ActiveTileChanged -= OnActiveTileChanged;
    }

    private void OnActiveTileChanged(TileBase activeTile)
    {
        background.color = activeTile == tile
            ? selectedColor
            : regularColor;

        if (editorState.ActiveTool != ToolType.Brush
            || editorState.ActiveTool != ToolType.RectBrush)
        {
            editorState.ActiveTool = ToolType.Brush;
        }
    }
}
