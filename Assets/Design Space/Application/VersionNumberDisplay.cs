using System;
using TMPro;
using UnityEngine;

public class VersionNumberDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private string displayFormat;
    
    private void Awake()
    {
        display.text = string.Format(displayFormat, Application.version);
    }
}
