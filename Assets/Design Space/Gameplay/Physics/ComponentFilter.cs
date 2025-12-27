using System;
using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Component Filter")]
public class ComponentFilter : GameObjectFilter
{
    [SerializeField] private string componentType;
    
    public override bool Filter(GameObject gameObject)
    {
        var type = Type.GetType(componentType);

        return gameObject.GetComponent(type) != null;
    }
}