using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime.Settings;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System;
using TMPro;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Enum Setting Display")]
public class EnumSettingDisplay : SettingDisplay
{
    [SerializeField] private IntSetting setting;
    [SerializeField] private EnumTypes enumType;
    [SerializeField] private List<TMP_Dropdown.OptionData> optionOverrides;

    public IntSetting Setting
    {
        get => setting;
        set => setting = value;
    }

    public List<TMP_Dropdown.OptionData> Options => optionOverrides.Count > 0
        ? optionOverrides
        : EnumTypeOptions[enumType].Select(name => new TMP_Dropdown.OptionData(name)).ToList();

    private enum EnumTypes
    {
        AntialiasingMode = 0,
        AntialiasingQuality = 1,
        Resolutions = 2,
        AutoSaveOptions = 3,
    }

    private static string[] GetNames(Type enumerable) => Enum.GetNames(enumerable);

    private static Dictionary<EnumTypes, string[]> EnumTypeOptions => new()
    {
        {
            EnumTypes.AntialiasingMode,
            GetNames(typeof(AntialiasingMode))
        },
        {
            EnumTypes.AntialiasingQuality,
            GetNames(typeof(AntialiasingQuality))
        },
        {
            EnumTypes.Resolutions,
            Screen.resolutions.Select(resolution => $"{resolution.width} X {resolution.height} at {resolution.refreshRateRatio.value:00.0}Hz").ToArray()
        },
        {
            EnumTypes.AutoSaveOptions,
            GetNames(typeof(AutosaveOptions))
        }
    };

    public override bool DefaultValue => setting.Value == setting.DefaultValue;

    public override void ResetValue()
    {
        setting.ResetToDefault();
    }
}
