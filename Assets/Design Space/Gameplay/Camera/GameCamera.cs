using System;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sizeBuffer;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minSize;
    [SerializeField] private TilePlacer placer;
    [SerializeField] private PerspectiveCameraPositioning cameraPositioning;
    [SerializeField] private Camera mainCamera;

    private Vector2 velocity;
    private float zoomVelocity;

    private void Update()
    {
        if (CameraTrackable.trackables.Count == 0) return;
        
        var trackableBounds = new Bounds(CameraTrackable.trackables[0].transform.position, Vector3.zero);
        
        foreach (var trackable in CameraTrackable.trackables)
        {
            trackableBounds.Encapsulate(trackable.transform.position);
        }
        
        float zoomTarget = Mathf.Max(minSize, GetZoomForSize(trackableBounds.size + Vector3.right * sizeBuffer));

        cameraPositioning.virtualHeight =
            Mathf.SmoothDamp(cameraPositioning.virtualHeight, zoomTarget, ref zoomVelocity, zoomSpeed);
        
        Vector2 position =
            Vector2.SmoothDamp(transform.position, trackableBounds.center, ref velocity, moveSpeed);

        var levelBounds = placer.Bounds;
        Vector2 cameraRect = mainCamera.rect.size * new Vector2((float)Screen.width / Screen.height, 1f);
        Vector2 cameraExtents = mainCamera.orthographicSize * cameraRect;
        Vector2 min = (Vector2)levelBounds.min + cameraExtents;
        Vector2 max = (Vector2)levelBounds.max - cameraExtents;

        position.x = min.x > max.x ? (min.x + max.x) / 2f : Mathf.Clamp(position.x, min.x, max.x);
        position.y = min.y > max.y ? (min.y + max.y) / 2f : Mathf.Clamp(position.y, min.y, max.y);

        transform.position = position;
    }
    
    private float GetZoomForSize(Vector2 size) => Mathf.Max(size.y, size.x * Screen.height / Screen.width);
}
