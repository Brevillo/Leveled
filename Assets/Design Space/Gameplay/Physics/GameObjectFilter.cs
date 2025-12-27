using UnityEngine;

public abstract class GameObjectFilter : ScriptableObject
{
    protected const string CreateAssetMenuPath = ProjectConstants.CommonFolder + "GameObject Filters/";
    
    public abstract bool Filter(GameObject gameObject);
}