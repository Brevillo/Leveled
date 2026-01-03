using UnityEngine;

public class GeneratePaletteCategories : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GenerateCategoryItems categoryItemPrefab;
    [SerializeField] private GameTilePalette palette;

    private void Awake()
    {
        foreach (var category in palette.Categories)
        {
            Instantiate(categoryItemPrefab, content).Initialize(category);
        }
    }
}