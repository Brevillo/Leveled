using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrackable : MonoBehaviour
{
    public static List<CameraTrackable> trackables = new();
    
    private void OnEnable()
    {
        trackables.Add(this);
    }

    private void OnDisable()
    {
        trackables.Remove(this);
    }
}
