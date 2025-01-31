using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelResizer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform resizer;
    [SerializeField] private RectTransform screenRect;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Image background;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color idleColor;
    [SerializeField] private float fadeInDistance;
    [SerializeField] private float snapWidth;

    private Vector2 dragPositionDelta;
    private bool dragging;

    private Vector2 MouseViewport => Input.mousePosition / new Vector2(Screen.width, Screen.height);

    private void Update()
    {
        // Resize camera rect
        Vector2 resizerPosition = resizer.anchoredPosition / screenRect.rect.size;
        mainCamera.rect = new(resizerPosition.x, 0, 1 - resizerPosition.x, 1);
        
        // Resize panel
        Vector2 panelSize = panel.sizeDelta;
        panelSize.x = resizer.anchoredPosition.x;
        panel.sizeDelta = panelSize;

        // Set background color
        var rect = resizer.rect;
        Vector2 mousePosition = MouseViewport * screenRect.rect.size - resizer.anchoredPosition;
        float distance = mousePosition.x < rect.center.x
            ? Mathf.InverseLerp(rect.xMin, rect.xMin - fadeInDistance, mousePosition.x)
            : Mathf.InverseLerp(rect.xMax, rect.xMax + fadeInDistance, mousePosition.x);
        background.color = Color.Lerp(selectedColor, idleColor, distance);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragPositionDelta = resizer.anchoredPosition - screenRect.rect.size * MouseViewport;
        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        resizer.anchoredPosition =
            Vector2.right * Mathf.Max(0, (MouseViewport * screenRect.rect.size + dragPositionDelta).x);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (dragging) return;
        
        TogglePanel();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
    }

    public void TogglePanel()
    {
        resizer.anchoredPosition = Vector2.right * (resizer.anchoredPosition.x < snapWidth / 2f
            ? snapWidth
            : 0);
    }
}
