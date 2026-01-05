using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ChangeloggedValue), true), CanEditMultipleObjects]
public class ChangeloggedValueEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        
        InspectorElement.FillDefaultInspector(root, serializedObject, this);
        
        root.Add(MyEditorUtilities.CreateDefaultManagerGUI<ChangeloggedValue, Changelog>(
            targets,
            changelog => changelog.ChangeloggedValues));

        return root;
    }
}
