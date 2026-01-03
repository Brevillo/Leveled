using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectOption : MonoBehaviour
{
    [SerializeField] private Button select;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image selectedIcon;
    [SerializeField] private Button delete;
    [SerializeField] private Button viewFile;
    [SerializeField] private Changelog changelog;

    private bool selected;

    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            selectedIcon.enabled = selected;
            select.interactable = !selected;
        }
    }
    
    public Button Select => select;
    public Button Delete => delete;
    public Button ViewFile => viewFile;

    public void Initialize(string levelName)
    {
        label.text = levelName;
    }
}
