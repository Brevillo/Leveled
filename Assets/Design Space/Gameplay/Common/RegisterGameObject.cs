using UnityEngine;

public class RegisterGameObject : MonoBehaviour
{
    [SerializeField] private GameObjectRegistry registry;

    private void OnEnable()
    {
        registry.Register(gameObject);
    }

    private void OnDisable()
    {
        registry.Deregister(gameObject);
    }
}