using System;
using UnityEngine;

public class DestroyOutOfLevelBounds : MonoBehaviour
{
    [SerializeField] private TilePlacerReference tilePlacerReference;

    private void Update()
    {
        if (!tilePlacerReference.value.Rect.Contains(transform.position))
        {
            Destroy(gameObject);
        }
    }
}
