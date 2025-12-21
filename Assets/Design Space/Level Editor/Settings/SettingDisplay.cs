using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SettingDisplay : ScriptableObject
{
    protected const string CreateAssetMenuPath = ProjectConstants.SettingsFolder;
    
    [SerializeField] private string displayName;
    [SerializeField] private SettingsSection section;
    
    public string DisplayName
    {
        get => displayName;
        set => displayName = value;
    }

    public SettingsSection Section => section;

    public abstract bool DefaultValue { get; }

    public abstract void ResetValue();
}
