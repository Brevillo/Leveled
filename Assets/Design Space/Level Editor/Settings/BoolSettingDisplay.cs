using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime.Settings;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Bool Setting Display")]
public class BoolSettingDisplay : SettingDisplay
{
    [SerializeField] private BoolSetting setting;

    public BoolSetting Setting
    {
        get => setting;
        set => setting = value;
    }

    public override bool DefaultValue => setting.Value == setting.DefaultValue;

    public override void ResetValue()
    {
        setting.ResetToDefault();
    }
}
