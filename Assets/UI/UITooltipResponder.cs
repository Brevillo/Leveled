using System;
using TMPro;
using UnityEngine;

public class UITooltipResponder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private GameObject contents;
    [SerializeField] private RectTransform screenRect;
    [SerializeField] private Camera mainCamera;

    private UITooltip currentTooltip;
    
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
        contents.SetActive(currentTooltip != null);

        if (currentTooltip != null)
        {
            var target = (RectTransform)currentTooltip.transform;
            
            contents.transform.position = target.TransformPoint(target.rect.min);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        currentTooltip = null;
    }
}
