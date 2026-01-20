using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelResizer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform resizer;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private float defaultSnapWidth;
    [SerializeField] private float toggleMoveSpeed;
    [SerializeField] private float cameraAdjustAnchoring;
    [SerializeField] private RectTransform screenRect;
    
    private Vector2 dragPositionDelta;
    private bool dragging;
    private float snapWidth;
    
    private float previousDragWidth;
    private int dragDirection;

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
            
            // Move panel
            resizer.anchoredPosition = Vector2.right * value;
            
            // Resize panel
            float realSize = Mathf.Max(value, defaultSnapWidth);
            panel.sizeDelta = new Vector2(realSize, panel.sizeDelta.y);
            panel.anchoredPosition = Vector2.left * (realSize - value);

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

    private void Start()
    {
        Width = targetWidth = snapWidth = defaultSnapWidth;
    }
    
    private void Update()
    {
        Width = Mathf.SmoothDamp(Width, targetWidth, ref widthVelocity, toggleMoveSpeed);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        dragPositionDelta = resizer.anchoredPosition - ScreenSize * spaceUtility.MouseWindow;
        previousDragWidth = Width;
        dragDirection = 1;
        dragging = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Width = targetWidth = snapWidth = Mathf.Max(0, (spaceUtility.MouseWindow * ScreenSize + dragPositionDelta).x);

        int newDragDirection = (Width - previousDragWidth).Sign0();
        if (newDragDirection != 0)
        {
            dragDirection = newDragDirection;
        }
        
        previousDragWidth = Width;
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

        if (Width < defaultSnapWidth)
        {
            targetWidth = dragDirection > 0f
                ? defaultSnapWidth
                : 0f;

            snapWidth = defaultSnapWidth;
        }
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
