using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;
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
    
    public void CenterCameraOnLevel()
    {
        if (gameStateManager.GameState == GameState.Playing) return;
        
        transform.position = placer.Bounds.center;

        SetSize(placer.Bounds.size * cameraSnapBufferMult);
    }

    private float GetZoomForSize(Vector2 size) => Mathf.Max(size.y, size.x * (Screen.height * mainCamera.rect.height) / (Screen.width * mainCamera.rect.width)) / 2f;
}
