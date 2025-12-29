using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float zoomFactor;
    [SerializeField] private float maxZoom;
    [SerializeField] private float minZoom;
    [SerializeField] private float zoomSmoothing;
    [SerializeField] private float defaultZoom;
    [SerializeField] private TilePlacer placer;
    [SerializeField] private float cameraSnapBufferMult;

    private float zoomTarget;
    private float zoomVelocity;
    
    private Vector2 playPositionVelocity;

    public void SetSize(Vector2 size)
    {
        float zoom = size == Vector2.zero ? defaultZoom : GetZoomForSize(size);
        
        zoom = Mathf.Clamp(zoom, maxZoom, minZoom);
        
        zoomTarget = zoom;
        mainCamera.orthographicSize = zoom;
    }
    
    private void Awake()
    {
        SetSize(Vector2.up * defaultZoom);
    }

    private void Update()
    {
        bool pointerOverUI = editorState.PointerOverUI;
        
        if (!pointerOverUI)
        {
            float newZoom = Mathf.Pow(Mathf.Pow(zoomTarget, 1f / zoomFactor) - Input.mouseScrollDelta.y * zoomSpeed, zoomFactor);
            
            zoomTarget = Mathf.Clamp(newZoom, maxZoom, minZoom);
        }
        
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, zoomTarget, ref zoomVelocity, zoomSmoothing);
        
        transform.position += mouseWorldPosition - mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    public void CenterCameraOnLevel()
    {
        if (gameStateManager.EditorState == EditorState.Playing) return;
        
        transform.position = placer.Bounds.center;

        SetSize(placer.Bounds.size * cameraSnapBufferMult);
    }

    private float GetZoomForSize(Vector2 size)
    {
        var cameraRect = mainCamera.rect;
        return Mathf.Max(size.y, size.x * Screen.height / Screen.width * cameraRect.height / cameraRect.width) / 2f;
    }
}
