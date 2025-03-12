using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Editor Systems/Positions Recording")]
public class PositionRecording : ScriptableObject
{
    public class Segment
    {
        public readonly List<Vector3> positions = new();
    }
    
    private List<Segment> segments;

    public List<Segment> Segments => segments ??= new();

    public event Action<PositionRecording> RecordingUpdated;
    public event Action<PositionRecording, Segment> SegmentUpdated;
    
    public Segment ActiveSegment
    {
        get
        {
            if (Segments.Count == 0)
            {
                NewSegment();
            }
            
            return Segments[^1];
        }
    }

    public void NewSegment()
    {
        Segments.Add(new());
    }
    
    public void Add(Vector3 position)
    {
        ActiveSegment.positions.Add(position);
        SegmentUpdated?.Invoke(this, ActiveSegment);
    }

    public void Clear()
    {
        Segments.Clear();
        RecordingUpdated?.Invoke(this);
    }
}
