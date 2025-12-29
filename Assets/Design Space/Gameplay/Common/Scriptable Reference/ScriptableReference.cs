using System;
using UnityEngine;

public abstract class ScriptableReference<T> : ScriptableObject
{
    protected const string CreateAssetMenuPath = ProjectConstants.CommonFolder + "Scriptable References/";

    public T value;
}