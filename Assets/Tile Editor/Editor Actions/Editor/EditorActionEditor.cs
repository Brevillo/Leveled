using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EditorAction))]
public class EditorActionEditor : Editor
{
    private EditorActionsManager manager;

    private EditorAction EditorAction => (EditorAction)target;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        MyEditorUtilities.DefaultManagerGUI(ref manager,
            manager => manager.EditorActions.Contains(EditorAction),
            manager => manager.EditorActions.Add(EditorAction));
    }
}
