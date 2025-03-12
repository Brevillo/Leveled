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

    public static event Action<UITooltip> TooltipRequested;
    public static event Action<UITooltip> TooltipCanceled;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        TooltipRequested?.Invoke(this);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        TooltipCanceled?.Invoke(this);
    }
}