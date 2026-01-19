using System.Collections.Generic;
using UnityEngine;

public class PhysicsContactChild : MonoBehaviour
{
    private readonly List<PhysicsContactParent> parents = new();
    
    public void AddParent(PhysicsContactParent parent)
    {
        if (parents.Contains(parent)) return;
        
        parents.Add(parent);
        parent.AddChild(this);
        
        ResolveActiveParent();
    }

    public void RemoveParent(PhysicsContactParent parent)
    {
        if (!parents.Contains(parent)) return;
        
        parents.Remove(parent);
        parent.RemoveChild(this);
        
        ResolveActiveParent();
    }

    private void ResolveActiveParent()
    {
        if (!enabled || (parents.Count > 0 && !parents[0].enabled)) return;
        
        transform.parent = parents.Count > 0 ? parents[0].transform : null;
    }
}