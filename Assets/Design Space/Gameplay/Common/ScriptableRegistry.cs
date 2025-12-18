using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptableRegistry<T> : ScriptableObject
{
    protected const string CreateAssetMenuPath = ProjectConstants.CommonFolder + "Scriptable Registries/";

    private HashSet<T> registrants = new();

    public IReadOnlyCollection<T> Registrants => registrants;

    public int Count => registrants.Count;
    
    public void Register(T registrant)
    {
        registrants.Add(registrant);
    }

    public void Deregister(T registrant)
    {
        registrants.Remove(registrant);
    }
}