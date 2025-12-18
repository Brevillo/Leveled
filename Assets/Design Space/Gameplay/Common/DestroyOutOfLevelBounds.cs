using System;
using UnityEngine;

public class DestroyOutOfLevelBounds : MonoBehaviour
{
    [SerializeField] private TilePlacerReference tilePlacerReference;

    private void Update()
    {
        if (!tilePlacerReference.tilePlacer.Bounds.Contains(transform.position))
        {
            Destroy(gameObject);
        }
    }
}
