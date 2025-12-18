using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoolSettingHub : SettingHub<BoolSettingDisplay>
{
    [SerializeField] private SimpleToggle toggle;

    protected override Selectable Initialize()
    {
        toggle.onValueChanged.AddListener(OnValueChanged);
        toggle.IsOn = display.Setting.Value;

        display.Setting.ValueChanged += toggle.SetIsOnWithoutNotify;

        return toggle;
    }

    private void OnDestroy()
    {
        display.Setting.ValueChanged -= toggle.SetIsOnWithoutNotify;
    }

    private void OnValueChanged(bool value)
    {
        display.Setting.Value = value;
    }
}
