using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleToggle : Selectable, IPointerClickHandler
{
    public Graphic toggleGraphic;
    public UnityEvent<bool> onValueChanged;

    private bool isOn;

    public bool IsOn
    {
        get => isOn;
        set => SetIsOnWithNotify(value);
    }

    public void SetIsOnWithoutNotify(bool value)
    {
        isOn = value;
    }

    private void SetIsOnWithNotify(bool value)
    {
        isOn = value;
        toggleGraphic.enabled = isOn;
        onValueChanged.Invoke(value);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        SetIsOnWithNotify(!isOn);
    }
}
