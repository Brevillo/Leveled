using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string contents;

    public string Contents
    {
        get => contents;
        set => contents = value;
    }

    private void OnDisable()
    {
        TooltipCanceled?.Invoke(this);
    }

    public static event Action<UITooltip> TooltipRequested;
    public static event Action<UITooltip> TooltipCanceled;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (contents == "") return;
        
        TooltipRequested?.Invoke(this);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (contents == "") return;
        
        TooltipCanceled?.Invoke(this);
    }
}