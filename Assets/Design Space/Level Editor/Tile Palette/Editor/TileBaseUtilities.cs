using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileBaseUtilities
{
    private const string CreateTilePath = ProjectConstants.CreateAssetMenuValidators + "Create Tile from Sprite";
    
    [MenuItem(CreateTilePath, true)]
    private static bool Validate() => Selection.objects.Any(selection => selection is Sprite || MyEditorUtilities.GetSubAssets<Sprite>(selection).Length > 0);

    [MenuItem(CreateTilePath, priority = ProjectConstants.CreateAssetMenuValidatorsPriority)]
    private static void Create()
    {
        string folder = MyEditorUtilities.GetActiveFolderPath();

        var tiles = Selection.objects
            .Select(selection =>
            {
                var sprite = selection as Sprite ?? MyEditorUtilities.GetSubAssets<Sprite>(selection).First();
                    
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;

                AssetDatabase.CreateAsset(tile, $"{folder}/{sprite.name}.asset");

                return (Object)tile;
            })
            .ToArray();
        
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.objects = tiles;
    }
}
