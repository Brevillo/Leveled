using System;
using OliverBeebe.UnityUtilities.Runtime.Settings;
using TMPro;
using UnityEngine;

public class EnumSettingUI : MonoBehaviour
{
    [SerializeField] private IntSetting setting;
    [SerializeField] private TMP_Dropdown dropdown;
    
    private void Awake()
    {
        dropdown.SetValueWithoutNotify(setting.Value);
        
        dropdown.onValueChanged.AddListener(value => setting.Value = value);
        setting.ValueChanged += dropdown.SetValueWithoutNotify;
    }
}