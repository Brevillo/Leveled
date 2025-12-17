using System;
using UnityEngine;

public class TileGameObject : MonoBehaviour
{
    [SerializeField] private GameObject context;

    public bool Active
    {
        get => context.activeSelf;
        set => context.SetActive(value);
    }

    private void Awake()
    {
        Active = true;
    }
}
