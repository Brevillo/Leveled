using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectOption : MonoBehaviour
{
    [SerializeField] private Button select;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image selectedIcon;
    [SerializeField] private Image dirtyIcon;
    [SerializeField] private Button delete;
    [SerializeField] private Changelog changelog;

    private string levelName; 
    private bool selected;

    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            selectedIcon.enabled = selected;
            UpdateDirty();
        }
    }
    
    public Button Select => select;
    public Button Delete => delete;

    private void OnEnable()
    {
        changelog.LogUpdated += OnChangelogUpdated;
    }

    private void OnDisable()
    {
        changelog.LogUpdated -= OnChangelogUpdated;
    }

    private void OnChangelogUpdated()
    {
        UpdateDirty();
    }

    private void UpdateDirty()
    {
        dirtyIcon.enabled = Selected && changelog.ActiveLevelDirty;
    }

    public void Initialize(string levelName)
    {
        this.levelName = levelName;
        label.text = levelName;
        UpdateDirty();
    }
}
