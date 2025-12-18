using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBoundsGizmo : MonoBehaviour
{
    [SerializeField] private Color color;
    
    private void OnDrawGizmosSelected()
    {
        var tilemap = GetComponent<Tilemap>();

        if (tilemap == null)
        {
            return;
        }
        
        var bounds = tilemap.localBounds;
        bounds.center = transform.TransformPoint(tilemap.localBounds.center);
        
        Gizmos.color = color;
        Gizmos.DrawCube(bounds.center, bounds.size);
    }
}
