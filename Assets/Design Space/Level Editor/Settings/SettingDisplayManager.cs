using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.SettingsFolder + "Setting Display List")]
public class SettingDisplayManager : ScriptableObject
{
    [SerializeField] private List<SettingDisplay> settingDisplays;

    public List<SettingDisplay> SettingDisplays => settingDisplays;
}
