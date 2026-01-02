using UnityEditor;
using UnityEditor.EventSystems;
using UnityEngine;

[CustomEditor(typeof(EventTriggerExecutor))]
public class EventTriggerExecutorEditor : EventTriggerEditor
{
    private SerializedProperty passEventForwardProperty;

    protected override void OnEnable()
    {
        base.OnEnable();

        passEventForwardProperty = serializedObject.FindProperty(nameof(EventTriggerExecutor.passEventForward));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(passEventForwardProperty);
        serializedObject.ApplyModifiedProperties();
        
        base.OnInspectorGUI();
    }
}
