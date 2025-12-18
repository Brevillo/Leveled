using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public abstract class SettingHub : MonoBehaviour
{
    public abstract Selectable Initialize(SettingDisplay display);
}

public abstract class SettingHub<TDisplay> : SettingHub where TDisplay : SettingDisplay
{
    [SerializeField] protected TDisplay display;
    [Space]
    [SerializeField] private TextMeshProUGUI label;

    private void Awake()
    {
        if (display != null)
        {
            Initialize(display);
        }
    }

    public override Selectable Initialize(SettingDisplay display)
    {
        this.display = display as TDisplay;

        label.text = display.DisplayName;

        return Initialize();
    }

    protected abstract Selectable Initialize();
}
