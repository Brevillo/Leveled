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

    private string levelName;
    
    public Button Select => select;
    public Button Delete => delete;

    public void Initialize(string levelName)
    {
        this.levelName = levelName;
        label.text = levelName;
    }

    public void UpdateSelected(string loadedLevel, bool dirty)
    {
        bool selected = levelName == loadedLevel;
        selectedIcon.enabled = selected;
        dirtyIcon.enabled = selected && dirty;
    }
}
