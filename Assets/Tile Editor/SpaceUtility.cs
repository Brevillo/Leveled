using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpaceUtility : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Grid grid;

    public Camera Camera => mainCamera;
    public Grid Grid => grid;

    public Vector3 MouseScreen => Input.mousePosition;
    public Vector3 MouseWorld => mainCamera.ScreenToWorldPoint(Input.mousePosition);
    public Vector3 MouseViewport => mainCamera.ScreenToViewportPoint(Input.mousePosition);
    public Vector3 MouseWindow => ScreenToWindowPoint(Input.mousePosition);

    public Vector3Int MouseCell => grid.WorldToCell(MouseWorld);
    public Vector3 MouseCellCenterWorld => mainCamera.ScreenToWorldPoint(MouseCell);

    public Vector3 GetBoundsIntCenterWorld(BoundsInt bounds) =>
        (grid.GetCellCenterWorld(bounds.min) + grid.GetCellCenterWorld(bounds.max)) / 2f;

    public Vector3 GetCellCenterWorld(Vector3Int position) => grid.GetCellCenterWorld(position);

    public Vector3 WorldToWindowPoint(Vector3 position) =>
        ViewportToWindowPoint(mainCamera.WorldToViewportPoint(position));

    public Vector3 ScreenToWindowPoint(Vector3 position) =>
        ViewportToWindowPoint(mainCamera.ScreenToViewportPoint(position));

    public Vector3 ViewportToWindowPoint(Vector3 position) =>
        (Vector2.one - (Vector2)position) * mainCamera.rect.min + (Vector2)position;

    public Vector3 WindowToWorldPoint(Vector3 position) =>
        mainCamera.ViewportToWorldPoint(WindowToViewportPoint(position));

    public Vector3 WindowToViewportPoint(Vector3 position) =>
        ((Vector2)position - Vector2.one) / mainCamera.rect.size + Vector2.one;

    public Vector3 CanvasToWindowPoint(Vector3 position, RectTransform rectTransform)
    {
        var screenRect = GetScreenRect(rectTransform);
        Vector2 canvasPoint = rectTransform.TransformPoint(rectTransform.rect.center) / screenRect.localScale.x +
                              position;
        return canvasPoint / screenRect.rect.size;
    }

    /// <summary>
    /// Clamps a RectTransform to the window. Assumes the canvas is screen space. 
    /// </summary>
    /// <param name="rectTransform"> The RectTransform to clamp. </param>
    /// <param name="windowPoint"> The position to clamp to in window space. </param>
    /// <returns> The clamped position in the local space of the RectTransform</returns>
    public Vector3 ClampRectTransformToWindow(RectTransform rectTransform, Vector3 windowPoint)
    {
        var screenRect = GetScreenRect(rectTransform);
        
        var transformSize = rectTransform.rect.size;
        var screenExtent = screenRect.rect.size / 2f;
        
        Vector2 position = screenExtent * 2f * ((Vector2)windowPoint - Vector2.one / 2f);
        Vector2 max = screenExtent - transformSize * (Vector2.one - rectTransform.pivot);
        Vector2 min = screenExtent - transformSize * rectTransform.pivot;
        
        position.x = Mathf.Clamp(position.x, -min.x, max.x);
        position.y = Mathf.Clamp(position.y, -min.y, max.y);

        return position;
    }

    public RectTransform GetScreenRect(RectTransform rectTransform)
    {
        if (screenRects.TryGetValue(rectTransform, out var screenRect))
        {
            return screenRect;
        }

        screenRect = rectTransform.root.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        screenRects.Add(rectTransform, screenRect);

        return screenRect;
    }

    private readonly Dictionary<RectTransform, RectTransform> screenRects = new();
}
