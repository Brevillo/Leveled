using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BalatroCardEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float lookTargetOffset;
    [SerializeField] private float rotateStrength;
    [SerializeField] private float horizontalTilt;
    [SerializeField] private float hoverScale;
    [SerializeField] private float rotationSmoothing;
    [SerializeField] private RectTransform card;
    [SerializeField] private SpaceUtility spaceUtility;
    
    private bool hovered;
    private Vector3 forward;
    
    private Vector3 rotationVelocity;
    private float scaleVelocity;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }

    private void Update()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = 0f;
        Vector3 center = spaceUtility.CanvasToWindow(card.position, card) * new Vector2(Screen.width, Screen.height);
        center.z = -lookTargetOffset;
        
        Vector3 targetForward = hovered
            ? Vector3.Lerp(Vector3.forward, (mouse - center).normalized, rotateStrength)
            : Vector3.forward;
        
        forward = Vector3.SmoothDamp(forward, targetForward, ref rotationVelocity, rotationSmoothing);
        card.forward = forward;
        card.eulerAngles += Vector3.forward * (horizontalTilt * targetForward.x);
        
        float targetScale = hovered
            ? hoverScale
            : 1f;

        card.localScale = Vector3.one *
                          Mathf.SmoothDamp(card.localScale.x, targetScale, ref scaleVelocity, rotationSmoothing);
    }
}
