using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LinkingGroupViewportVisual : MonoBehaviour
{
    [SerializeField] private MetadataViewportVisual metadataViewportVisual;
    [SerializeField] private TextMeshProUGUI groupDisplay;
    [SerializeField] private RectTransform connectionDisplay;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private float startDistance;
    [SerializeField] private float endDistance;
    [SerializeField] private Vector2 endPosition;
    [SerializeField] private TileEditorState tileEditorState;
    [SerializeField] private Changelog changelog;
    
    private static readonly List<LinkingGroupViewportVisual> visuals = new();
    private static Dictionary<Vector2Int, Vector2Int> connections;
    
    private void Awake()
    {
        metadataViewportVisual.MetadataUpdated += OnMetadataUpdated;
        metadataViewportVisual.PositionUpdated += OnPositionUpdated;
        
        changelog.ChangeEvent += OnChangeEvent;

        visuals.Add(this);

        connections ??= LinkingGroup.GetAllConnections(tileEditorState.LevelInstance);
    }

    private void OnDestroy()
    {
        changelog.ChangeEvent -= OnChangeEvent;

        visuals.Remove(this);
    }

    private void OnChangeEvent(ChangeInfo changeInfo)
    {
        if (visuals.FirstOrDefault() != this) return;
        
        switch (changeInfo)
        {
            case TileChangeInfo:

                connections = LinkingGroup.GetAllConnections(tileEditorState.LevelInstance);
                
                break;
        }
    }

    private void OnMetadataUpdated(object metadata)
    {
        var linkingGroup = (LinkingGroup)metadata;
        
        groupDisplay.text = linkingGroup.groupID;
    }

    private void OnPositionUpdated()
    {
        var rectTransform = (RectTransform)transform;
        
        Vector2 connectionCanvas = spaceUtility.CellToCanvas(connections[metadataViewportVisual.CellPosition], rectTransform);
        Vector2 selfCanvas = rectTransform.position;

        Vector2 startOffset = connectionCanvas - selfCanvas;
        startOffset *= 0.5f * Mathf.Min(
            startOffset.x == 0f ? 1f : rectTransform.sizeDelta.x / Mathf.Abs(startOffset.x),
            startOffset.y == 0f ? 1f : rectTransform.sizeDelta.y / Mathf.Abs(startOffset.y));
        Vector2 start = selfCanvas + startOffset;

        Vector2 endOffset = selfCanvas - connectionCanvas;
        endOffset *= 0.5f * Mathf.Min(
            endOffset.x == 0f ? 1f : rectTransform.sizeDelta.x / Mathf.Abs(endOffset.x),
            endOffset.y == 0f ? 1f : rectTransform.sizeDelta.y / Mathf.Abs(endOffset.y));
        Vector2 end = connectionCanvas + endOffset;

        Vector2 startToEnd = (end - start).normalized;
        start += startToEnd * startDistance;
        end -= startToEnd * endDistance;

        float length = Mathf.Max(0f, Vector2.Dot(end - start, (connectionCanvas - selfCanvas).normalized));
        
        connectionDisplay.position = (start + end) / 2f;
        connectionDisplay.right = end - start;
        connectionDisplay.sizeDelta = new(length, connectionDisplay.sizeDelta.y);
    }
}
