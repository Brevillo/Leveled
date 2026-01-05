using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectedTilesUI : MonoBehaviour
{
    [SerializeField] private Image primaryTileIcon;
    [SerializeField] private Image secondaryTileIcon;
    [SerializeField] private Sprite noTileSprite;
    [SerializeField] private Changelog changelog;
    [SerializeField] private ChangeloggedGameTile primaryTile;
    [SerializeField] private ChangeloggedGameTile secondaryTile;

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
        secondaryTileIcon.sprite = secondaryTile.Value != null 
            ? secondaryTile.Value.PaletteIcon
            : noTileSprite;
        
        primaryTileIcon.sprite = primaryTile.Value != null 
            ? primaryTile.Value.PaletteIcon
            : noTileSprite;
    }
}
