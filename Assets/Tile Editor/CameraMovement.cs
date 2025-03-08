using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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

    private float zoomTarget;
    private float zoomVelocity;

    private bool dragging;
    
    private Vector2 playPositionVelocity;

    private Vector2 mousePosition;

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
        bool pointerOverUI = editorState.PointerOverUI;
        
        if (!pointerOverUI)
        {
            bool moverTool = false;// editorState.ActiveTool == ToolType.Mover; // todo: implement mover tool

            ButtonControl
                middle = Mouse.current.middleButton,
                right = Mouse.current.rightButton,
                left = Mouse.current.leftButton;
            
            if (!dragging && (middle.wasPressedThisFrame || (moverTool && (left.wasPressedThisFrame || right.wasPressedThisFrame))))
            {
                mousePosition = Input.mousePosition;

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                dragging = true;
            }
            
            if (dragging && (middle.isPressed || (moverTool && (left.isPressed || right.isPressed))))
            {
                Vector2 delta = Input.mousePositionDelta * 4f;
                transform.position -= mainCamera.ScreenToWorldPoint(delta) - mainCamera.ScreenToWorldPoint(Vector3.zero);

                mousePosition += delta;
            }
            
            if (dragging && (middle.wasReleasedThisFrame || (moverTool && (left.wasReleasedThisFrame || right.wasReleasedThisFrame))))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                
                Vector2 viewportPosition = mainCamera.ScreenToViewportPoint(mousePosition);
                
                Mouse.current.WarpCursorPosition(
                        viewportPosition.x < 0 
                        || viewportPosition.x > 1
                        || viewportPosition.y < 0
                        || viewportPosition.y > 1
                    ? mainCamera.ViewportToScreenPoint(Vector2.one / 2f)
                    : mousePosition);

                dragging = false;
            }
        }
        
        if (!pointerOverUI)
        {
            float newZoom = Mathf.Pow(Mathf.Pow(zoomTarget, 1f / zoomFactor) - Input.mouseScrollDelta.y * zoomSpeed, zoomFactor);
            
            zoomTarget = Mathf.Clamp(newZoom, maxZoom, minZoom);
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
        
        zoomTarget = Mathf.Max(minPlaySize, GetZoomForSize(trackableBounds.size + Vector3.right * playSizeBuffer));
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
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, zoomTarget, ref zoomVelocity, zoomSmoothing);
    }

    private void SetZoom(float value)
    {
        value = Mathf.Clamp(value, maxZoom, minZoom);
        zoomTarget = value;
        mainCamera.orthographicSize = value;
    }
    
    public void CenterCameraOnLevel()
    {
        if (gameStateManager.GameState == GameState.Playing) return;
        
        var tilemaps = placer.Tilemaps;
        
        if (tilemaps.Length == 0)
        {
            return;
        }
        
        transform.position = placer.Bounds.center;
        SetZoom(placer.Bounds.size == Vector3.zero 
            ? defaultZoom 
            : GetZoomForSize(placer.Bounds.size));
    }

    private float GetZoomForSize(Vector2 size) => Mathf.Max(size.y, size.x * (Screen.height * mainCamera.rect.height) / (Screen.width * mainCamera.rect.width)) / 2f;
}
