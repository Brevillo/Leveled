using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TilePaletteItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Color primaryColor;
    [SerializeField] private Color secondaryColor;
    [SerializeField] private Color regularColor;
    [SerializeField] private Image iconImage;
    [SerializeField] private UITooltip tooltip;
    [SerializeField] private UnityEvent selected;
    [SerializeField] private UnityEvent hovered;
    [SerializeField] private ChangeloggedGameTile primaryTile;
    [SerializeField] private ChangeloggedGameTile secondaryTile;
    [SerializeField] private Changelog changelog;

    private GameTile gameTile;
    
    public void Initialize(GameTile tile)
    {
        iconImage.sprite = tile.PaletteIcon;
        gameTile = tile;
        tooltip.Contents = tile.Tooltip;
    }

    private void OnEnable()
    {
        changelog.ChangeEvent += OnChangeEvent;
    }

    private void OnDisable()
    {
        changelog.ChangeEvent -= OnChangeEvent;
    }

    private void OnChangeEvent(ChangeInfo changeInfo)
    {
        background.color
            = primaryTile.Value == gameTile ? primaryColor 
            : secondaryTile.Value == gameTile ? secondaryColor
            : regularColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selected.Invoke();
        
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:

                primaryTile.Value = gameTile;
                
                break;
            
            case PointerEventData.InputButton.Right:

                secondaryTile.Value = gameTile;
                
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered.Invoke();
    }
}
