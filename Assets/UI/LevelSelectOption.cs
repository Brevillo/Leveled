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

    public Button Select => select;
    public Button Delete => delete;

    public void Initialize(string levelName)
    {
        label.text = levelName;
    }

    public void UpdateSelected(string loadedLevel)
    {
        selectedIcon.enabled = label.text == loadedLevel;
    }
}
