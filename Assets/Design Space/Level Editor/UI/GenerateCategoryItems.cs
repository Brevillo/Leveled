using System;
using TMPro;
using UnityEngine;

public class GenerateCategoryItems : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private TilePaletteItem tileItemPrefab;
    [SerializeField] private TextMeshProUGUI headerTitleDisplay;
    
    public void Initialize(GameTileCategory category)
    {
        headerTitleDisplay.text = category.DisplayTitle;
        headerTitleDisplay.gameObject.SetActive(category.DisplayTitle != "");
        
        foreach (var tile in category.GameTiles)
        {
            Instantiate(tileItemPrefab, content).Initialize(tile);
        }
    }
}
