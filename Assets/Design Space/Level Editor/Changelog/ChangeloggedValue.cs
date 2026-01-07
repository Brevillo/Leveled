using System;
using UnityEngine;

public abstract class ChangeloggedValue : ScriptableObject
{
    public abstract void Initialize(Changelog changelog);
    public abstract void Cleanup();
}

public abstract class ChangeloggedValue<T> : ChangeloggedValue
{
    protected const string CreateAssetMenuPath = ProjectConstants.ChangelogFolder + "Value ";

    [SerializeField] private T defaultValue;
    [SerializeField] private string changelogName;
    [SerializeField] private bool log;
    
    private T value;

    private Changelog changelog;
    
    public override void Initialize(Changelog changelog)
    {
        this.changelog = changelog;
        
        changelog.StateUpdated += OnStateUpdated;
        
        Value = defaultValue;
    }

    public override void Cleanup()
    {
        changelog.StateUpdated -= OnStateUpdated;
    }

    private void OnStateUpdated(ChangeInfo changeInfo)
    {
        if (changeInfo is ValueChangeInfo<T> valueChangeInfo
            && valueChangeInfo.name == changelogName)
        {
            value = valueChangeInfo.newValue;
        }
    }

    public T Value
    {
        get => value;
        set => changelog.SendChange(
            new ValueChangeInfo<T>(
                name: changelogName,
                previousValue: this.value,
                newValue: value,
                $"Changed '{changelogName}' from {this.value} to {value}"),
            log);
    }
    
    public event Action<ChangeInfo> ChangeEvent
    {
        add => changelog.ChangeEvent += value;
        remove => changelog.ChangeEvent -= value;
    }

    public bool ThisChangeInfo(ChangeInfo changeInfo, out ValueChangeInfo<T> valueChangeInfo) =>
        (valueChangeInfo = changeInfo as ValueChangeInfo<T>) != null && valueChangeInfo.name == name;
}