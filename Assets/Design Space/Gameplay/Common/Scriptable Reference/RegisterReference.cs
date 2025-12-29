using UnityEngine;

public abstract class RegisterReference<TValue, TReference> : MonoBehaviour where TValue : Object where TReference : ScriptableReference<TValue> 
{
    [SerializeField] private TValue value;
    [SerializeField] private TReference reference;
    
    private void Awake()
    {
        reference.value = value;
    }
}