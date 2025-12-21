using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private SettingDisplayManager settings;
    [SerializeField] private BoolSettingHub boolSettingPrefab;
    [SerializeField] private FloatSettingHub floatSettingPrefab;
    [SerializeField] private EnumSettingHub enumSettingPrefab;
    [SerializeField] private GameObject sectionEntriesParentPrefab;
    [SerializeField] private Transform sectionsParent;
    [SerializeField] private EditorActionButton sectionButtonPrefab;
    [SerializeField] private Transform sectionButtonParent;
    [SerializeField] private SettingsSection defaultSection;
    [SerializeField] private TextMeshProUGUI selectedSectionDisplay;

    private Dictionary<SettingsSection, GameObject> settingSections;
    
    private void Start()
    {
        settingSections = new();

        var sectionButtons = new List<(SettingsSection section, Transform transform)>();
        
        foreach (var setting in settings.SettingDisplays)
        {
            SettingHub prefab = setting switch
            {
                BoolSettingDisplay => boolSettingPrefab,
                FloatSettingDisplay => floatSettingPrefab,
                EnumSettingDisplay => enumSettingPrefab,
                _ => null,
            };

            if (prefab == null) continue;

            if (!settingSections.TryGetValue(setting.Section, out var parent))
            {
                var sectionButton = Instantiate(sectionButtonPrefab, sectionButtonParent);
                sectionButton.SetEditorAction(setting.Section.SectionSelectEditorAction);
                sectionButton.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    SelectSection(setting.Section);
                });
                
                sectionButtons.Add((setting.Section, sectionButton.transform));

                parent = Instantiate(sectionEntriesParentPrefab, sectionsParent);
                
                settingSections.Add(setting.Section, parent);
            }

            var instance = Instantiate(prefab, parent.transform);
            instance.Initialize(setting);
        }

        foreach (var sectionButton in sectionButtons.OrderBy(sectionButton => sectionButton.section.Priority))
        {
            sectionButton.transform.SetAsLastSibling();
        }
        
        SelectSection(defaultSection);
    }

    private void SelectSection(SettingsSection selectedSection)
    {
        foreach (var section in settingSections)
        {
            section.Value.SetActive(section.Key == selectedSection);
        }

        selectedSectionDisplay.text = selectedSection.DisplayName;
    }
}