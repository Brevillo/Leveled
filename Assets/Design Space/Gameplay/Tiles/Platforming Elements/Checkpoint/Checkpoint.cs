using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private UnityEvent activated;
    [SerializeField] private UnityEvent deactivated;

    private bool active;
    
    public void Activate()
    {
        if (active) return;
        active = true;
        
        activated.Invoke();
    }

    public void Deactivate()
    {
        if (!active) return;
        active = false;
        
        deactivated.Invoke();
    }
}
