using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.SettingsFolder + "Section")]
public class SettingsSection : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private int priority;
    
    public string DisplayName => displayName;
    public int Priority => priority;
}