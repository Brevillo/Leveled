using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(GameTilePalette))]
public class GameTilePaletteEditor : Editor
{
    private GameTilePalette Palette => target as GameTilePalette;

    private VisualElement warnings;
    
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        var getAllTilesButton = new Button(GetAllTiles)
        {
            text = "Get All Tiles",
        };
        
        root.Add(getAllTilesButton);

        warnings = new();
        
        var tilesField = root.Q<PropertyField>("PropertyField:tiles");
        tilesField.RegisterValueChangeCallback(TilesValueChanged);
        
        root.Add(warnings);

        RefreshIDWarnings();
        
        return root;
    }

    private void TilesValueChanged(SerializedPropertyChangeEvent changeEvent)
    {
        RefreshIDWarnings();
    }

    private void RefreshIDWarnings()
    {
        warnings.Clear();

        var tiles = Palette.Tiles;
        
        foreach (var selfTile in tiles)
        {
            if (selfTile == null)
            {
                continue;
            }
            
            var sameID = tiles.Find(otherTile => otherTile.ID == selfTile.ID && otherTile != selfTile);
            
            if (sameID != null)
            {
                warnings.Add(new HelpBox($"{selfTile.name} and {sameID.name} have the same ID: {selfTile.ID}", HelpBoxMessageType.Error));
            }
        }
    }
    
    private void GetAllTiles()
    {
        var tilesProp = serializedObject.FindProperty("tiles");

        var tiles = AssetDatabase.FindAssets($"t:{nameof(GameTile)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<GameTile>)
            .Where(tile => tile != null)
            .ToArray();
        
        // skip to array fields
        tilesProp.Next(true);
        tilesProp.Next(true);

        tilesProp.intValue = tiles.Length;
        
        // advance to first array item
        tilesProp.Next(true);

        for (int i = 0; i < tiles.Length; i++)
        {
            tilesProp.objectReferenceValue = tiles[i];

            if (i < tiles.Length - 1)
            {
                tilesProp.Next(false);
            }
        }

        serializedObject.ApplyModifiedProperties();
        
        RefreshIDWarnings();
    }
}
