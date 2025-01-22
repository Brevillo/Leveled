using System;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    [SerializeField] private float minPlaySize;
    [SerializeField] private float playMoveSpeed;
    [SerializeField] private float playSizeBuffer;
    
    private Vector3 prevMousePosition;

    private float zoomTarget;
    private float zoomVelocity;

    private Vector2 playPositionVelocity;

    private void Awake()
    {
        SetZoom(defaultZoom);
    }

    private void Update()
    {
        switch (gameStateManager.GameState)
        {
            case GameState.Editing:
                EditModeCamera();
                break;
            
            case GameState.Playing:
                PlayModeCamera();
                break;
        }
    }

    private void EditModeCamera()
    {
        if (!editorState.PointerOverUI)
        {
            if (Input.GetMouseButton(2)
                || (editorState.ActiveTool == ToolType.Mover && Input.GetMouseButton(0)))
            {
                transform.position -= mainCamera.ScreenToWorldPoint(Input.mousePosition - prevMousePosition) - mainCamera.ScreenToWorldPoint(Vector3.zero);
            }
            
            prevMousePosition = Input.mousePosition;
            
            zoomTarget = Mathf.Pow(Mathf.Clamp(Mathf.Pow(zoomTarget - Input.mouseScrollDelta.y * zoomSpeed, zoomFactor), maxZoom, minZoom), 1f / zoomFactor);
        }
        
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        UpdateCameraZoom();
        transform.position += mouseWorldPosition - mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }
    
    private void PlayModeCamera()
    {
        if (CameraTrackable.trackables.Count == 0) return;
        
        var trackableBounds = new Bounds(CameraTrackable.trackables[0].transform.position, Vector3.zero);
        
        foreach (var trackable in CameraTrackable.trackables)
        {
            trackableBounds.Encapsulate(trackable.transform.position);
        }
        
        zoomTarget = Mathf.Pow(Mathf.Max(minPlaySize, GetZoomForSize(trackableBounds.size + Vector3.right * playSizeBuffer)), 1f / zoomFactor);
        UpdateCameraZoom();
        
        Vector2 position =
            Vector2.SmoothDamp(transform.position, trackableBounds.center, ref playPositionVelocity, playMoveSpeed);

        var levelBounds = placer.Bounds;
        Vector2 cameraRect = mainCamera.rect.size * new Vector2((float)Screen.width / Screen.height, 1f);
        Vector2 cameraExtents = mainCamera.orthographicSize * cameraRect;
        Vector2 min = (Vector2)levelBounds.min + cameraExtents;
        Vector2 max = (Vector2)levelBounds.max - cameraExtents;

        position.x = min.x > max.x ? (min.x + max.x) / 2f : Mathf.Clamp(position.x, min.x, max.x);
        position.y = min.y > max.y ? (min.y + max.y) / 2f : Mathf.Clamp(position.y, min.y, max.y);

        transform.position = position;
    }

    private void UpdateCameraZoom()
    {
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, Mathf.Pow(zoomTarget, zoomFactor), ref zoomVelocity, zoomSmoothing);
    }

    private void SetZoom(float value)
    {
        zoomTarget = Mathf.Pow(value, 1f / zoomFactor);
        mainCamera.orthographicSize = value;
    }
    
    public void CenterCameraOnLevel()
    {
        var tilemaps = placer.Tilemaps;

        if (tilemaps.Length == 0)
        {
            return;
        }
        
        transform.position = placer.Bounds.center;
        SetZoom(GetZoomForSize(placer.Bounds.size));
    }

    private float GetZoomForSize(Vector3 size) => Mathf.Max(size.y, size.x * (Screen.height * mainCamera.rect.height) / (Screen.width * mainCamera.rect.width)) / 2f;
}
