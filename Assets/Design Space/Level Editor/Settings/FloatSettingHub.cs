using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatSettingHub : SettingHub<FloatSettingDisplay>
{
    [SerializeField] private TextMeshProUGUI number;
    [SerializeField] private Slider slider;

    protected override Selectable Initialize()
    {
        slider.maxValue = display.Maximum / display.StepValue;
        slider.minValue = display.Minimum / display.StepValue;

        slider.onValueChanged.AddListener(OnValueChanged);
        OnSettingValueChanged(display.Setting.Value);

        display.Setting.ValueChanged += OnSettingValueChanged;

        return slider;
    }

    private void OnDestroy()
    {
        display.Setting.ValueChanged -= OnSettingValueChanged;
    }

    private void OnValueChanged(float displayValue)
    {
        display.Setting.Value = displayValue * display.StepValue;
    }

    private void OnSettingValueChanged(float realValue)
    {
        slider.SetValueWithoutNotify(realValue / display.StepValue);
        number.text = $"{Mathf.RoundToInt(realValue * display.DisplayValueMultiple)}{display.NumberSuffix}";
    }
}
