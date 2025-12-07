using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Space Utility")]
public class SpaceUtility : ScriptableObject
{
    /* Space Utility Options
    
    Screen Space
        Pixel space of the screen
        (0,0) to (Screen.width, Screen.height)
     
    World Space
        Global space for all GameObjects
        Unbounded
    
    Viewport Space
        Normalized space for the portion of the screen rendered to by the main camera
        (0,0) to (1,1)
    
    Window Space
        Normalized space for the entire screen
        (0, 0) to (1, 1)
        
    Cell Space
        Local space for the grid in integer coordinates (Vector2Ints)
        Unbounded
        
    Canvas Space
        Local space for the root canvas of a given RectTransform
        -canvas.pixelRect.size / 2 to canvas.pixelRect.size / 2
    
     */
    
    private Camera mainCamera;
    private Grid grid;

    public Camera Camera => mainCamera;
    public Grid Grid => grid;

    public void Setup(Camera mainCamera, Grid grid)
    {
        this.mainCamera = mainCamera;
        this.grid = grid;
    }
    
    #region Mouse Positions
    
    public Vector3 MouseScreen => Input.mousePosition;
    public Vector3 MouseWorld => mainCamera.ScreenToWorldPoint(Input.mousePosition);
    public Vector3 MouseViewport => mainCamera.ScreenToViewportPoint(Input.mousePosition);
    public Vector3 MouseWindow => ScreenToWindow(Input.mousePosition);

    public Vector3 MouseCanvas(RectTransform rectTransform) => ScreenToCanvas(MouseScreen, rectTransform);
    
    public Vector2Int MouseCell => (Vector2Int)grid.WorldToCell(MouseWorld);
    public Vector3 MouseCellCenterWorld => grid.GetCellCenterWorld((Vector3Int)MouseCell);

    #endregion
    
    #region to World

    public Vector3 ScreenToWorld(Vector3 position) =>
        mainCamera.ScreenToWorldPoint(position);

    public Vector3 ViewportToWorld(Vector3 position) =>
        mainCamera.ViewportToWorldPoint(position);
    
    public Vector3 WindowToWorld(Vector3 position) =>
        mainCamera.ViewportToWorldPoint(WindowToViewport(position));
    
    public Vector3 CellToWorld(Vector2Int position) => 
        grid.GetCellCenterWorld((Vector3Int)position);
    
    public Vector3 CanvasToWorld(Vector3 position, RectTransform rectTransform) =>
        WindowToWorld(CanvasToWindow(position, rectTransform));
    
    #endregion
    
    #region to Viewport

    public Vector3 ScreenToViewport(Vector3 position) =>
        mainCamera.ScreenToViewportPoint(position);

    public Vector3 WorldToViewport(Vector3 position) =>
        mainCamera.WorldToViewportPoint(position);
    
    public Vector3 WindowToViewport(Vector3 position) =>
        ((Vector2)position - Vector2.one) / mainCamera.rect.size + Vector2.one;

    public Vector3 CellToViewport(Vector2Int position) =>
        WorldToViewport(CellToWorld(position)); // cell -> world -> viewport

    public Vector3 CanvasToViewport(Vector3 position, RectTransform rectTransform) =>
        WindowToViewport(CanvasToWindow(position, rectTransform)); // canvas -> window -> viewport
        
    #endregion
    
    #region To Window

    public Vector3 ScreenToWindow(Vector3 position) =>
        new Vector3(position.x / Screen.width, position.y / Screen.height, 0f);
    
    public Vector3 WorldToWindow(Vector3 position) =>
        ViewportToWindow(WorldToViewport(position)); // world -> viewport -> window

    public Vector3 ViewportToWindow(Vector3 position) =>
        (Vector2.one - (Vector2)position) * mainCamera.rect.min + (Vector2)position;

    public Vector3 CellToWindow(Vector2Int position) =>
        ViewportToWindow(CellToViewport(position)); // cell -> viewport -> window
    
    public Vector3 CanvasToWindow(Vector3 position, RectTransform rectTransform)
    {
        var canvas = GetCanvas(rectTransform);
        
        Vector2 screenPoint = canvas.transform.InverseTransformPoint(position);
        Vector2 windowPoint = screenPoint / canvas.pixelRect.size + Vector2.one / 2f;
        
        return windowPoint;
    }

    #endregion
    
    #region To Cell

    public Vector2Int ScreenToCell(Vector3 position) =>
        WorldToCell(ScreenToWorld(position)); // screen -> world -> cell 

    public Vector2Int WorldToCell(Vector3 position) =>
        (Vector2Int)grid.WorldToCell(position);
    
    public Vector3 SnapWorldToCell(Vector3 position) =>
        grid.CellToWorld(grid.WorldToCell(position));

    public Vector2Int ViewportToCell(Vector3 position) =>
        WorldToCell(ViewportToWorld(position)); // viewport -> world -> cell

    public Vector2Int WindowToCell(Vector3 position) =>
        WorldToCell(WindowToWorld(position)); // window -> world -> cell

    public Vector2Int CanvasToCell(Vector3 position, RectTransform rectTransform) =>
        WorldToCell(CanvasToWorld(position, rectTransform)); // canvas -> world -> cell

    #region BoundsInt
    
    public Vector3 GetBoundsIntCenterWorld(BoundsInt bounds) => GetBoundsIntAnchorWorld(bounds, Vector2.one / 2f);

    public Vector3 GetBoundsIntAnchorWorld(BoundsInt bounds, Vector2 anchor)
    {
        Vector2 min = grid.GetCellCenterWorld(bounds.min) - grid.cellSize / 2f;
        Vector2 max = grid.GetCellCenterWorld(bounds.max - Vector3Int.one) + grid.cellSize / 2f;

        return new Vector2(
            Mathf.LerpUnclamped(min.x, max.x, anchor.x),
            Mathf.LerpUnclamped(min.y, max.y, anchor.y));
    }

    public Vector3 GetBoundsIntCenterCanvas(BoundsInt bounds, RectTransform rectTransform) =>
        WorldToCanvas(GetBoundsIntCenterWorld(bounds), rectTransform); // cell -> world -> canvas
    
    #endregion
    
    #endregion
    
    #region to Canvas

    public Vector3 ScreenToCanvas(Vector3 position, RectTransform rectTransform) =>
        WindowToCanvas(ScreenToWindow(position), rectTransform); // screen -> window -> canvas
    
    public Vector3 WorldToCanvas(Vector3 position, RectTransform rectTransform) =>
        WindowToCanvas(WorldToWindow(position), rectTransform); // world -> window -> canvas
    
    public Vector3 ViewportToCanvas(Vector3 position, RectTransform rectTransform) =>
        WindowToCanvas(ViewportToWindow(position), rectTransform); // viewport -> window -> canvas
    
    public Vector3 WindowToCanvas(Vector3 position, RectTransform rectTransform) =>
        position * GetCanvas(rectTransform).pixelRect.size;

    public Vector3 CellToCanvas(Vector2Int position, RectTransform rectTransform) =>
        WorldToCanvas(CellToWorld(position), rectTransform); // cell -> world -> canvas
    
    #region Clamping
    
    public Vector3 ClampCanvasPointToCanvasRect(Vector3 position, RectTransform rectTransform) =>
        ClampCanvasPointToCanvasRect(position, rectTransform, Vector2.zero);
    public Vector3 ClampCanvasPointToCanvasRect(Vector3 position, RectTransform rectTransform, Vector2 buffer)
    {
        var canvas = GetCanvas(rectTransform);

        Vector2 max = canvas.pixelRect.size - rectTransform.rect.size * (Vector2.one - rectTransform.pivot) - buffer;
        Vector2 min = rectTransform.rect.size * rectTransform.pivot + buffer;
        
        position.x = Mathf.Clamp(position.x, min.x, max.x);
        position.y = Mathf.Clamp(position.y, min.y, max.y);

        return position;
    }

    #endregion
    
    public Canvas GetCanvas(RectTransform rectTransform)
    {
        if (canvases.TryGetValue(rectTransform, out var canvas))
        {
            return canvas;
        }

        var parentCanvas = rectTransform.GetComponentInParent<Canvas>();
        canvas = parentCanvas.rootCanvas;
        canvases.Add(rectTransform, canvas);

        return canvas;
    }

    private readonly Dictionary<RectTransform, Canvas> canvases = new();
    
    #endregion
}
