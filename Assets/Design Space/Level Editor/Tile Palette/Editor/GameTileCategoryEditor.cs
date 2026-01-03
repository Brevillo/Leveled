using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(GameTileCategory)), CanEditMultipleObjects]
public class GameTileCategoryEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        root.Add(MyEditorUtilities.CreateDefaultManagerGUI<GameTileCategory, GameTilePalette>(targets,
            palette => palette.Categories));
        
        return root;
    }
}