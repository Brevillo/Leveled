using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class EnumSettingHub : SettingHub<EnumSettingDisplay>
{
    [SerializeField] private TMP_Dropdown dropdown;

    protected override Selectable Initialize()
    {
        dropdown.options = display.Options;
        
        display.Setting.ValueChanged += dropdown.SetValueWithoutNotify;
        
        dropdown.onValueChanged.AddListener(OnValueChanged);
        dropdown.value = display.Setting.Value;
        
        return dropdown;
    }

    private void OnDestroy()
    {
        display.Setting.ValueChanged -= dropdown.SetValueWithoutNotify;
    }

    private void OnValueChanged(int displayValue)
    {
        display.Setting.Value = displayValue;
    }
}
