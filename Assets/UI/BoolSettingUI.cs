using OliverBeebe.UnityUtilities.Runtime.Settings;
using UnityEngine;
using UnityEngine.UI;

public class BoolSettingUI : MonoBehaviour
{
    [SerializeField] private BoolSetting setting;
    [SerializeField] private Toggle toggle;

    private void Awake()
    {
        toggle.SetIsOnWithoutNotify(setting.Value);
        
        toggle.onValueChanged.AddListener(value => setting.Value = value);
        setting.ValueChanged += toggle.SetIsOnWithoutNotify;
    }
}