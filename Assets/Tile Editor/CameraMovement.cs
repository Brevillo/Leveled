using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomSpeed;    
    [SerializeField] private float maxZoom;
    [SerializeField] private float zoomSmoothing;
    [SerializeField] private float defaultZoom;
    
    private Vector3 prevMousePosition;

    private float zoomTarget;
    private float zoomVelocity;

    private void Awake()
    {
        zoomTarget = Mathf.Pow(defaultZoom, 0.1f);
    }

    private void Update()
    {
        if (Input.GetMouseButton(2)
            || (editorState.ActiveTool == ToolType.Mover && Input.GetMouseButton(0)))
        {
            transform.position -= mainCamera.ScreenToWorldPoint(Input.mousePosition - prevMousePosition) - mainCamera.ScreenToWorldPoint(Vector3.zero);
        }
        prevMousePosition = Input.mousePosition;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        zoomTarget = Mathf.Max(Mathf.Pow(maxZoom, 0.1f), zoomTarget - Input.mouseScrollDelta.y * zoomSpeed);
        mainCamera.orthographicSize =
            Mathf.SmoothDamp(mainCamera.orthographicSize, Mathf.Pow(zoomTarget, 10), ref zoomVelocity, zoomSmoothing);
        
        transform.position += mouseWorldPosition - mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }
}
