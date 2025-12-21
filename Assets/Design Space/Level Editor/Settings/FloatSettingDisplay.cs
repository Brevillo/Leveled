using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime.Settings;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Float Setting Display")]
public class FloatSettingDisplay : SettingDisplay
{
    [SerializeField] private FloatSetting setting;
    [SerializeField] private float minimum;
    [SerializeField] private float maximum;
    [SerializeField] private float stepValue;
    [SerializeField] private float displayValueMultiple;
    [SerializeField] private string numberSuffix;

    public FloatSetting Setting
    {
        get => setting;
        set => setting = value;
    }

    public float Minimum => minimum;
    public float Maximum => maximum;
    public float StepValue => stepValue;
    public float DisplayValueMultiple => displayValueMultiple;
    public string NumberSuffix => numberSuffix;

    public override bool DefaultValue => setting.Value == setting.DefaultValue;

    public override void ResetValue()
    {
        setting.ResetToDefault();
    }
}
