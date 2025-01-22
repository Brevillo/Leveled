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
            text = "Add Missing Tiles",
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
        var tiles = Palette.Tiles;
        
        var newTiles = AssetDatabase.FindAssets($"t:{nameof(GameTile)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<GameTile>)
            .Where(tile => tile != null)
            .Where(tile => !tiles.Contains(tile))
            .ToArray();

        Undo.RecordObject(target, "Added tiles to Tile Palette");
        tiles.AddRange(newTiles);

        serializedObject.ApplyModifiedProperties();
        
        RefreshIDWarnings();
    }
}
