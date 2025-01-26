using System;
using UnityEngine;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject content;

    private void Awake()
    {
        content.SetActive(false);
    }

    public void ToggleVisibility()
    {
        content.SetActive(!content.activeSelf);
    }

    public void CloseSettings()
    {
        content.SetActive(false);
    }

    public void OpenSettings()
    {
        content.SetActive(true);
    }
}
