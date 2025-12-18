using System;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChangelogEntryItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private Image background;
    [SerializeField] private Poolable poolable;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private Color activeBackgroundColor;
    [SerializeField] private Color inactiveBackgroundColor;
    [SerializeField] private Button button;
    
    public void SetActive(bool active)
    {
        textMesh.color = active ? activeColor : inactiveColor;
        
        button.enabled = !active;
        background.color = active ? activeBackgroundColor : inactiveBackgroundColor;
    }

    public void Setup(string text, Action clicked)
    {
        textMesh.text = text;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(clicked.Invoke);
    }

    public void Return() => poolable.Return();
}
