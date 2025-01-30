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
    public Vector3 MouseCellCenterWorld => grid.GetCellCenterWorld(MouseCell);

    public Vector3 GetBoundsIntCenterWorld(BoundsInt bounds) =>
        (grid.GetCellCenterWorld(bounds.min) + grid.GetCellCenterWorld(bounds.max)) / 2f;

    public Vector3 GetCellCenterWorld(Vector3Int position) => grid.GetCellCenterWorld(position);

    public Vector3 WorldToWindowPoint(Vector3 position) =>
        ViewportToWindowPoint(mainCamera.WorldToViewportPoint(position));

    public Vector3 ScreenToWindowPoint(Vector3 position) =>
        ViewportToWindowPoint(mainCamera.ScreenToViewportPoint(position));
    
    public Vector3 CanvasToWindowPoint(Vector3 position, RectTransform rectTransform)
    {
        var canvas = GetCanvas(rectTransform);
        
        Vector2 screenPoint = canvas.transform.InverseTransformPoint(position);
        Vector2 windowPoint = screenPoint / canvas.pixelRect.size + Vector2.one / 2f;
        
        return windowPoint;
    }

    public Vector3 ViewportToWindowPoint(Vector3 position) =>
        (Vector2.one - (Vector2)position) * mainCamera.rect.min + (Vector2)position;

    public Vector3 WindowToWorldPoint(Vector3 position) =>
        mainCamera.ViewportToWorldPoint(WindowToViewportPoint(position));

    public Vector3 WindowToViewportPoint(Vector3 position) =>
        ((Vector2)position - Vector2.one) / mainCamera.rect.size + Vector2.one;
    
    public Vector3 ClampWorldCanvasPointToCanvasRect(Vector3 position, RectTransform rectTransform)
    {
        var canvas = GetCanvas(rectTransform);

        Vector2 max = canvas.pixelRect.size - rectTransform.rect.size * (Vector2.one - rectTransform.pivot);
        
        position.x = Mathf.Clamp(position.x, 0, max.x);
        position.y = Mathf.Clamp(position.y, 0, max.y);

        return position;
    }

    public Canvas GetCanvas(RectTransform rectTransform)
    {
        if (canvases.TryGetValue(rectTransform, out var canvas))
        {
            return canvas;
        }

        canvas = rectTransform.root.GetComponentInChildren<Canvas>();
        canvases.Add(rectTransform, canvas);

        return canvas;
    }

    private readonly Dictionary<RectTransform, Canvas> canvases = new();
}
