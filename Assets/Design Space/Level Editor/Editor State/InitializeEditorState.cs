using System;
using UnityEngine;

public class InitializeEditorState : MonoBehaviour
{
    [SerializeField] private Changelog changelog;

    private void Awake()
    {
        changelog.Initialize();
    }

    private void OnDestroy()
    {
        changelog.Cleanup();
    }
}