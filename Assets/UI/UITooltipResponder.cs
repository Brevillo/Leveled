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
    [SerializeField] private float inactiveSnapDuration;
    
    private UITooltip currentTooltip;
    private Vector2 velocity;
    private float inactiveTimer;
    
    private Vector2 TargetWindowPosition
    {
        get
        {
            var target = (RectTransform)currentTooltip.transform;
            RectTransform window = (RectTransform)contents.transform;
            
            Vector2 center = target.TransformPoint(target.rect.center + (tooltipAnchor - Vector2.one / 2f) * target.rect.size);
            Vector2 position = spaceUtility.ClampWorldCanvasPointToCanvasRect(center, window);

            return position;
        }
    }
    
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
            if (inactiveTimer > inactiveSnapDuration)
            {
                contents.transform.position = TargetWindowPosition;
            }

            inactiveTimer = 0f;
            
            contents.transform.position = Vector2.SmoothDamp(contents.transform.position, TargetWindowPosition, ref velocity, moveSpeed);
        }
        else
        {
            inactiveTimer += Time.deltaTime;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        currentTooltip = null;
    }
}
