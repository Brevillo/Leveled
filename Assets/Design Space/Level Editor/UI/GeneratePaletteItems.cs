using System;
using UnityEngine;

public class GeneratePaletteItems : MonoBehaviour
{
    [SerializeField] private GameTilePalette palette;
    [SerializeField] private Transform content;
    [SerializeField] private TilePaletteItem tileItemPrefab;
    
    private void Awake()
    {
        foreach (var tile in palette.Tiles)
        {
            Instantiate(tileItemPrefab, content).Initialize(tile);
        }
    }
}
