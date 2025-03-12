using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelResizer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform resizer;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private Image background;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color idleColor;
    [SerializeField] private float fadeInDistance;
    [SerializeField] private float defaultSnapWidth;
    [SerializeField] private float toggleMoveSpeed;
    [SerializeField] private float cameraAdjustAnchoring;
    [SerializeField] private RectTransform screenRect;
    
    private Vector2 dragPositionDelta;
    private bool dragging;
    private float snapWidth;

    private float targetWidth;
    private float widthVelocity;

    public event Action<bool> PanelToggled;
    
    private Vector2 ScreenSize => screenRect.rect.size;
    
    private float Width
    {
        get => resizer.anchoredPosition.x;
        set
        {
            float initialValue = resizer.anchoredPosition.x;
            
            // spaceUtility.Camera.transform.position -=
            //     (spaceUtility.CanvasToWorld(resizer.anchoredPosition, resizer) 
            //      - spaceUtility.CanvasToWorld(Vector2.right * value, resizer)) * cameraAdjustAnchoring;
            
            // Resize camera rect
            float resizerPosition = value / ScreenSize.x;
            // spaceUtility.Camera.rect = new(resizerPosition, 0, 1 - resizerPosition, 1);
            
            // Move panel
            resizer.anchoredPosition = Vector2.right * value;
            
            // Resize panel
            Vector2 panelSize = panel.sizeDelta;
            panelSize.x = value;
            panel.sizeDelta = panelSize;

            if (initialValue == 0 && value != 0)
            {
                PanelToggled?.Invoke(true);
            }
            else if (value == 0 && initialValue != 0)
            {
                PanelToggled?.Invoke(false);
            }
        }
    }

    private void Awake()
    {
        snapWidth = defaultSnapWidth;
    }
    
    private void Update()
    {
        Width = Mathf.SmoothDamp(Width, targetWidth, ref widthVelocity, toggleMoveSpeed);

        // Set background color
        var rect = resizer.rect;
        float mousePosition = spaceUtility.MouseWindow.x * ScreenSize.x - Width;
        float distance = mousePosition < rect.center.x
            ? Mathf.InverseLerp(rect.xMin, rect.xMin - fadeInDistance, mousePosition)
            : Mathf.InverseLerp(rect.xMax, rect.xMax + fadeInDistance, mousePosition);
        
        background.color = Color.Lerp(selectedColor, idleColor, distance);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        dragPositionDelta = resizer.anchoredPosition - ScreenSize * spaceUtility.MouseWindow;
        dragging = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Width = targetWidth = snapWidth = Mathf.Max(0, (spaceUtility.MouseWindow * ScreenSize + dragPositionDelta).x);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (dragging) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            targetWidth = snapWidth = defaultSnapWidth;
        }
        else
        {
            TogglePanel();
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
    }

    public void TogglePanel()
    {
        if (targetWidth > 0)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    public void ClosePanel()
    {
        if (targetWidth < defaultSnapWidth / 2f)
        {
            snapWidth = defaultSnapWidth;
        }

        targetWidth = 0;

        PanelToggled?.Invoke(false);
    }

    public void OpenPanel()
    {
        targetWidth = snapWidth;
        PanelToggled?.Invoke(true);
    }
}
