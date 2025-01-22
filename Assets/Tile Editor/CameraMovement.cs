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
                DampZoom();
                transform.position += mouseWorldPosition - mainCamera.ScreenToWorldPoint(Input.mousePosition);

                break;
            
            case GameState.Playing:

                if (CameraTrackable.trackables.Count == 0) break;
                
                var bounds = new Bounds(CameraTrackable.trackables[0].transform.position, Vector3.zero);

                foreach (var trackable in CameraTrackable.trackables)
                {
                    bounds.Encapsulate(trackable.transform.position);
                }

                transform.position = Vector2.SmoothDamp(transform.position, bounds.center, ref playPositionVelocity,
                    playMoveSpeed);
                
                zoomTarget = Mathf.Pow(Mathf.Max(minPlaySize, GetZoomForSize(bounds.size + Vector3.right * playSizeBuffer)), 1f / zoomFactor);
                DampZoom();
                
                break;
        }

        void DampZoom()
        {
            mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, Mathf.Pow(zoomTarget, zoomFactor), ref zoomVelocity, zoomSmoothing);
        }
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
        
        var bounds = tilemaps[0].localBounds;
        
        foreach (var tilemap in tilemaps)
        {
            tilemap.CompressBounds();
            var localBounds = tilemap.localBounds;
            
            bounds.Encapsulate(new Bounds(tilemap.transform.TransformPoint(localBounds.center), localBounds.size));
        }
        
        transform.position = bounds.center;
        SetZoom(GetZoomForSize(bounds.size));
    }

    private float GetZoomForSize(Vector3 size) => Mathf.Max(size.y, size.x * (Screen.height * mainCamera.rect.height) / (Screen.width * mainCamera.rect.width)) / 2f;
}
