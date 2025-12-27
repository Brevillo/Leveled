using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using FloatField = UnityEngine.UIElements.FloatField;
using Vector2Field = UnityEngine.UIElements.Vector2Field;

[CustomPropertyDrawer(typeof(ContactFilter2D))]
public class ContactFilter2DEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new Foldout { text = property.displayName, };

        // Create Trigger Field
        
        root.Add(Field(nameof(ContactFilter2D.useTriggers)));

        // Create LayerMask Fields
        
        var layerMaskFieldsRow = new VisualElement
        {
            style = { flexDirection = FlexDirection.Row },
        };
        root.Add(layerMaskFieldsRow);
        
        var layerMaskField = new PropertyField(Property(nameof(ContactFilter2D.layerMask)))
        {
            style = { flexGrow = 1f },
        };
        
        ConditionalFields(layerMaskFieldsRow,
            nameof(ContactFilter2D.useLayerMask),
            new VisualElement[] { layerMaskField });

        // Create Depth Fields
        
        ConditionalFields(root,
            nameof(ContactFilter2D.useDepth),
            new[]
            {
                nameof(ContactFilter2D.minDepth),
                nameof(ContactFilter2D.maxDepth),
                nameof(ContactFilter2D.useOutsideDepth),
            }.Select(Field).ToArray<VisualElement>());

        // Create Normal Angle Fields

        var minNormalAngleProperty = Property(nameof(ContactFilter2D.minNormalAngle));
        var maxNormalAngleProperty = Property(nameof(ContactFilter2D.maxNormalAngle));

        float normalAngleRad = (minNormalAngleProperty.floatValue + maxNormalAngleProperty.floatValue) / 2f *
                               Mathf.Deg2Rad;
        
        Vector2 normal = new Vector2(Mathf.Cos(normalAngleRad), Mathf.Sin(normalAngleRad));
        normal /= Mathf.Abs(normal.x);

        var normalField = new Vector2Field
        {
            label = "Normal",
            value = normal,
        };
        normalField.AddToClassList("unity-base-field__aligned");

        float angleRange = maxNormalAngleProperty.floatValue - minNormalAngleProperty.floatValue;
        
        var normalAngleRangeField = new FloatField
        {
            label = "Normal Angle Range",
            value = angleRange,
        };
        normalAngleRangeField.AddToClassList("unity-base-field__aligned");
        
        normalField.RegisterValueChangedCallback(_ => UpdateNormalAngles());
        normalAngleRangeField.RegisterValueChangedCallback(_ => UpdateNormalAngles());
        
        void UpdateNormalAngles()
        {
            float anchor = Vector2.SignedAngle(Vector2.right, normalField.value);
            float halfRange = normalAngleRangeField.value / 2f;

            minNormalAngleProperty.floatValue = anchor - halfRange;
            maxNormalAngleProperty.floatValue = anchor + halfRange;

            property.serializedObject.ApplyModifiedProperties();
        }
        
        ConditionalFields(root,
            nameof(ContactFilter2D.useNormalAngle),
            new VisualElement[]
            {
                normalField,
                normalAngleRangeField,
                Field(nameof(ContactFilter2D.useOutsideNormalAngle)),
            });

        return root;

        SerializedProperty Property(string name) => property.FindPropertyRelative(name);
        PropertyField Field(string name) => new(Property(name));

        void ConditionalFields(VisualElement root, string name, VisualElement[] conditionalFields)
        {
            // Create and register fields
            var propertyField = Field(name);
            propertyField.style.flexGrow = 1f;

            propertyField.RegisterValueChangeCallback(changeEvent =>
            {
                bool visible = changeEvent.changedProperty.boolValue;
                
                foreach (var field in conditionalFields)
                {
                    field.SetDisplayed(visible);
                }
            });
            
            // Add fields

            root.Add(propertyField);
            
            foreach (var field in conditionalFields)
            {
                root.Add(field);
            }
        }
    }
}
