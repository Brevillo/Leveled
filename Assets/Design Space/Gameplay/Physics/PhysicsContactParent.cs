using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicsContactParent : MonoBehaviour
{
    private readonly HashSet<PhysicsContactChild> children = new();
    
    public void AddChild(PhysicsContactChild child) => children.Add(child);
    public void RemoveChild(PhysicsContactChild child) => children.Remove(child);

    private void OnDestroy()
    {
        // Cache children to new array since removing this parent
        // from each child modifies children collection.
        foreach (var child in children.ToArray())
        {
            child.RemoveParent(this);
        }
    }
}