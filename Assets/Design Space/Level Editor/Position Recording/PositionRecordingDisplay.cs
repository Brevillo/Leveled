using System.Collections.Generic;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using UnityEngine;

public class PositionRecordingDisplay : MonoBehaviour
{
    [SerializeField] private List<PositionRecording> recordings;
    [SerializeField] private GameObjectPool lineRendererPool;
    [SerializeField] private ChangeloggedBool showPlayerPositionRecording;
    
    private class LineSegment
    {
        public readonly Poolable poolable;
        public readonly LineRenderer lineRenderer;
        public readonly PositionRecording.Segment segment;

        public LineSegment(Poolable poolable, PositionRecording.Segment segment)
        {
            this.poolable = poolable;
            lineRenderer = poolable.GetComponent<LineRenderer>();
            this.segment = segment;
        }
    }
    
    private Dictionary<PositionRecording, List<LineSegment>> recordingLines;

    private List<LineSegment> GetLineSegments(PositionRecording recording)
    {
        if (recordingLines.TryGetValue(recording, out var lineSegments)) return lineSegments;
        
        lineSegments = new();
        recordingLines.Add(recording, lineSegments);

        return lineSegments;
    }

    private void Awake()
    {
        recordingLines = new();
    }

    private void OnEnable()
    {
        showPlayerPositionRecording.ChangeEvent += OnChangeEvent;

        foreach (var recording in recordings)
        {
            recording.RecordingUpdated += OnRecordingRecordingUpdated;
            recording.SegmentUpdated += OnSegmentUpdated;
        }
    }

    private void OnDisable()
    {
        showPlayerPositionRecording.ChangeEvent -= OnChangeEvent;

        foreach (var recording in recordings)
        {
            recording.RecordingUpdated -= OnRecordingRecordingUpdated;
            recording.SegmentUpdated -= OnSegmentUpdated;
        }
    }

    private void OnChangeEvent(ChangeInfo changeInfo)
    {
        if (showPlayerPositionRecording.Value)
        {
            DisplayRecordings();
        }
        else
        {
            ClearRecordings();
        }
    }

    private void OnSegmentUpdated(PositionRecording recording, PositionRecording.Segment segment)
    {
        if (!showPlayerPositionRecording.Value) return;
        
        var lineSegments = GetLineSegments(recording);
        
        var lineSegment = lineSegments.Find(line => line.segment == segment);
        
        if (lineSegment == null)
        {
            lineSegment = new(lineRendererPool.Retrieve(), segment);
            lineSegments.Add(lineSegment);
        }
        
        lineSegment.lineRenderer.positionCount = segment.positions.Count;
        lineSegment.lineRenderer.SetPositions(segment.positions.ToArray());
    }
    
    private void OnRecordingRecordingUpdated(PositionRecording recording)
    {
        if (!showPlayerPositionRecording.Value) return;

        var lineSegments = GetLineSegments(recording);
        
        foreach (var line in lineSegments)
        {
            line.poolable.Return();
        }
        
        lineSegments.Clear();
        
        DisplayRecording(recording);
    }

    public void DisplayRecordings()
    {
        foreach (var recording in recordings)
        {
            DisplayRecording(recording);
        }
    }

    private void DisplayRecording(PositionRecording recording)
    {
        var lineSegments = GetLineSegments(recording);
        
        foreach (var segment in recording.Segments)
        {
            var line = lineRendererPool.Retrieve();

            var renderer = line.GetComponent<LineRenderer>();
            renderer.positionCount = segment.positions.Count;
            renderer.SetPositions(segment.positions.ToArray());
                
            lineSegments.Add(new(line, segment));
        }
    }

    public void ClearRecordings()
    {
        foreach (var lines in recordingLines.Values)
        {
            foreach (var line in lines)
            {
                line.poolable.Return();
            }
        }
        
        recordingLines.Clear();
    }
}
