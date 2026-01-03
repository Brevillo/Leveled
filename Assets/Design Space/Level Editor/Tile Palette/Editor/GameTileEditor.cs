using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(GameTile)), CanEditMultipleObjects]
public class GameTileEditor : Editor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) =>
        MyEditorUtilities.GetSpritePreview(
            ((GameTile)target).PaletteIcon,
            (assetPath, subAssets, width, height),
            base.RenderStaticPreview);

    private List<GameTile> gameTiles;

    private bool DuplicateID => gameTiles.Exists(tile => tile != target && tile.ID == ((GameTile)target).ID);
    
    private void OnEnable()
    {
        gameTiles = AssetDatabase.FindAssets($"t:{nameof(GameTile)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<GameTile>)
            .ToList();
    }

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        var idField = root.Q<PropertyField>("PropertyField:id");
        var idProperty = serializedObject.FindProperty("id");

        var generateID = new Button(() =>
        {
            idProperty.intValue = Enumerable.Range(0, gameTiles.Count)
                .First(id => !gameTiles.Exists(tile => tile.ID == id));
            
            serializedObject.ApplyModifiedProperties();
        })
        {
            text = "Generate Unique ID",
            enabledSelf = DuplicateID,
        };

        idField.RegisterValueChangeCallback(_ =>
        {
            generateID.SetEnabled(DuplicateID);
        });
        
        root.Insert(root.IndexOf(idField) + 1, generateID);

        root.Add(MyEditorUtilities.CreateDefaultManagerGUI<GameTile, GameTileCategory>(targets,
            category => category.GameTiles));

        return root;
    }
}