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
    
    private const float tooltipDelay = 0.1f; 

    private bool hovered;
    private float tooltipRequestTime;
    
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        tooltipRequestTime = Time.time + tooltipDelay;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        TooltipCanceled?.Invoke(this);
    }

    private void Update()
    {
        if (hovered && Time.time > tooltipRequestTime)
        {
            hovered = false;
            TooltipRequested?.Invoke(this);
        }
    }
}
