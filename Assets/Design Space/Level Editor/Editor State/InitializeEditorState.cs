using System;
using UnityEngine;

public class InitializeEditorState : MonoBehaviour
{
    [SerializeField] private Changelog changelog;

    private void Start()
    {
        changelog.Initialize();
    }

    private void OnDestroy()
    {
        changelog.Cleanup();
    }
}