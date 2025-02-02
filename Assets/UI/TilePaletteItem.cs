using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TilePaletteItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Color primaryColor;
    [SerializeField] private Color secondaryColor;
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
            case PaletteChangeInfo:
                
                background.color
                    = editorState.PrimaryTile == gameTile ? primaryColor 
                    : editorState.SecondaryTile == gameTile ? secondaryColor
                    : regularColor;

                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:

                editorState.PrimaryTile = gameTile;
                
                break;
            
            case PointerEventData.InputButton.Right:

                editorState.SecondaryTile = gameTile;
                
                break;
        }
    }
}
