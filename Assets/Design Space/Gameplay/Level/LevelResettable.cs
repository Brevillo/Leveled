using System;
using UnityEngine;
using UnityEngine.Events;

public class LevelResettable : MonoBehaviour
{
    [SerializeField] private LevelService levelService;
    [SerializeField] private UnityEvent reset;
    public event Action Reset;

    private void OnEnable()
    {
        levelService.RegisterResettable(this);
    }

    private void OnDisable()
    {
        levelService.DeregisterResettable(this);
    }

    public void DoReset()
    {
        Reset?.Invoke();
        reset.Invoke();
    }
}
