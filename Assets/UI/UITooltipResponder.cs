using System;
using OliverBeebe.UnityUtilities.Runtime.Settings;
using TMPro;
using UnityEngine;

public class UITooltipResponder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private GameObject contents;
    [SerializeField] private float moveSpeed;
    [SerializeField] private BoolSetting tooltipsEnabledSetting;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private Vector2 tooltipAnchor;
    
    private UITooltip currentTooltip;
    private Vector2 velocity;
    
    private void OnEnable()
    {
        UITooltip.TooltipRequested += OnTooltipRequested;
        UITooltip.TooltipCanceled += OnTooltipCanceled;
    }

    private void OnDisable()
    {
        UITooltip.TooltipRequested -= OnTooltipRequested;
        UITooltip.TooltipCanceled -= OnTooltipCanceled;
    }

    private void OnTooltipRequested(UITooltip tooltip)
    {
        currentTooltip = tooltip;
        
        textMesh.text = tooltip.Contents;
    }

    private void OnTooltipCanceled(UITooltip tooltip)
    {
        if (tooltip != currentTooltip) return;

        currentTooltip = null;
    }

    private void LateUpdate()
    {
        bool enabled = tooltipsEnabledSetting.Value && currentTooltip != null; 
        
        contents.SetActive(enabled);

        if (enabled)
        {
            var target = (RectTransform)currentTooltip.transform;
            RectTransform window = (RectTransform)contents.transform;
            
            Vector2 center = (tooltipAnchor - Vector2.one / 2f) * target.rect.size;
            Vector2 windowPosition = spaceUtility.CanvasToWindowPoint(center, target);
            Vector2 position = spaceUtility.ClampRectTransformToWindow(window, windowPosition);
            
            window.anchoredPosition = Vector2.SmoothDamp(window.anchoredPosition, position, ref velocity, moveSpeed);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        currentTooltip = null;
    }
}
