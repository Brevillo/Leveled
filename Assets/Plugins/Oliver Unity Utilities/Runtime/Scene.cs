/* Made by Oliver Beebe 2024 */

using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OliverBeebe.UnityUtilities.Runtime 
{
    [System.Serializable]
    public struct Scene
    {
        #if UNITY_EDITOR
        [SerializeField] private SceneAsset editorSceneAsset;
        #endif
        public string name;
        
        #if UNITY_EDITOR

        [Tooltip("ONLY USE IN EDITOR SCRIPTS")]
        public readonly SceneAsset GetSceneAsset_EditorOnly() => editorSceneAsset;

        [CustomPropertyDrawer(typeof(Scene))]
        private class ScenePropertyDrawer : PropertyDrawer 
        {
            public override VisualElement CreatePropertyGUI(SerializedProperty property)
            {
                var sceneField = new PropertyField(property.FindPropertyRelative(nameof(editorSceneAsset)))
                {
                    name = property.displayName,
                };
                
                sceneField.RegisterValueChangeCallback(changEvent =>
                {
                    if (changEvent.changedProperty.objectReferenceValue is not SceneAsset sceneAsset) return;
                    
                    property.FindPropertyRelative(nameof(name)).stringValue = sceneAsset.name;
                    property.serializedObject.ApplyModifiedProperties();
                });
                
                return sceneField;
            }
        }

        #endif
    }
}
