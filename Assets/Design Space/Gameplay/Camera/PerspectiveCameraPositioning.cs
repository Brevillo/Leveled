using System.Collections.Generic;
using UnityEngine;

public class PerspectiveCameraPositioning : MonoBehaviour
{
    [SerializeField] private Camera perspectiveCamera;
    [SerializeField] private List<Camera> orthographicCamera;
    [SerializeField] private Transform pivot;

    public float virtualHeight;

    public float PerspectiveDistance =>
        virtualHeight / 2f * Mathf.Tan((90f - perspectiveCamera.fieldOfView / 2f) * Mathf.Deg2Rad);
    
    [ContextMenu("Update Position")]
    private void LateUpdate()
    {
        pivot.localPosition = Vector3.back * PerspectiveDistance;

        foreach (var camera in orthographicCamera)
        {
            camera.orthographicSize = virtualHeight / 2f;
        }
    }
}
