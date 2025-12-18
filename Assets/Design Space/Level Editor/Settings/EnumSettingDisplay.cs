using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OliverBeebe.UnityUtilities.Runtime.Settings;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Enum Setting Display")]
public class EnumSettingDisplay : SettingDisplay
{
    [SerializeField] private IntSetting setting;
    [SerializeField] private EnumTypes enumType;

    public IntSetting Setting => setting;

    public string[] Options => EnumTypeOptions[enumType];

    private enum EnumTypes
    {
        AntialiasingMode,
        AntialiasingQuality,
        Resolutions,
    }

    private static string[] GetNames(Type enumerable) => Enum.GetNames(enumerable)
        // .Select(name => name.AddSpacesToSentence())
        .ToArray();

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
    };

    public override bool DefaultValue => setting.Value == setting.DefaultValue;

    public override void ResetValue()
    {
        setting.ResetToDefault();
    }
}
