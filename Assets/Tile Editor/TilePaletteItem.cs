using System;
using UnityEngine;
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
    }
    
    public void SetActiveTile()
    {
        editorState.ActiveTile = gameTile;
    }

    private void OnEnable()
    {
        editorState.EditorChanged += OnEditorChanged;
    }

    private void OnDisable()
    {
        editorState.EditorChanged -= OnEditorChanged;
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case PaletteChangeInfo paletteChangeInfo:
                
                background.color = paletteChangeInfo.newTile == gameTile
                    ? selectedColor
                    : regularColor;

                if (editorState.ActiveTool != ToolType.Brush
                    && editorState.ActiveTool != ToolType.RectBrush)
                {
                    editorState.ActiveTool = ToolType.Brush;
                }

                break;
        }
    }
}
