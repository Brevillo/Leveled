using System;
using System.Collections.Generic;
using UnityEngine;

public class PositionRecorder : MonoBehaviour
{
    [SerializeField] private float recordInterval;
    [SerializeField] private float minDistance;
    [SerializeField] private PositionRecording recording;
    
    private float recordTimer;

    private void Start()
    {
        recording.Clear();
    }

    public void AddPosition()
    {
        recording.Add(transform.position);
        recordTimer = 0f;
    }

    public void NewSegment()
    {
        recording.NewSegment();
        AddPosition();
    }

    private void Update()
    {
        recordTimer += Time.deltaTime;

        if (recordTimer >= recordInterval
            && (recording.ActiveSegment.positions.Count == 0 || Vector3.Distance(transform.position, recording.ActiveSegment.positions[^1]) >= minDistance))
        {
            AddPosition();
        }
    }
}
