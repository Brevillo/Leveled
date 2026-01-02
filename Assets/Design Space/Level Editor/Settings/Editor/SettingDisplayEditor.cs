using System;
using System.Collections.Generic;
using System.Linq;
using OliverBeebe.UnityUtilities.Runtime.Settings;
using UnityEditor;
using Object = UnityEngine.Object;

[CustomEditor(typeof(SettingDisplay), true), CanEditMultipleObjects]
public class SettingDisplayEditor : Editor
{
    private SettingDisplayManager manager;
    
    private SettingDisplay SettingDisplay => target as SettingDisplay;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MyEditorUtilities.DefaultManagerGUI(
            ref manager,
            manager => manager.SettingDisplays.Any(setting => targets.Contains(setting)),
            manager => manager.SettingDisplays.AddRange(targets
                .OfType<SettingDisplay>()
                .Where(target => !manager.SettingDisplays.Contains(target))));
    }
    
    private const string CreateSettingDisplayPath = ProjectConstants.CreateAssetMenuValidators + "Create Setting Display";

    [MenuItem(CreateSettingDisplayPath, true)]
    private static bool Validate() => Selection.objects.Any(selection => selection is BoolSetting or FloatSetting or IntSetting);

    [MenuItem(CreateSettingDisplayPath, priority = ProjectConstants.CreateAssetMenuValidatorsPriority)]
    private static void Create()
    {
        var created = new List<Object>();
        
        created.AddRange(Create<BoolSetting, BoolSettingDisplay>((setting, display) => display.Setting = setting));
        created.AddRange(Create<FloatSetting, FloatSettingDisplay>((setting, display) => display.Setting = setting));
        created.AddRange(Create<IntSetting, EnumSettingDisplay>((setting, display) => display.Setting = setting));

        Selection.objects = created.ToArray();
    }
    
    private static Object[] Create<TSetting, TDisplay>(Action<TSetting, TDisplay> setupAction) 
        where TSetting : Object where TDisplay : SettingDisplay =>
        MyEditorUtilities.CreateScriptableObjectFromAssetSelection<TSetting, TDisplay>(
            (setting, display) =>
            {
                display.DisplayName = setting.name;
                setupAction.Invoke(setting, display);
            },
            setting => $"{setting.name} Display",
            false);
}
