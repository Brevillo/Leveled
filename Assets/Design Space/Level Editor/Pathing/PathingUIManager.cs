using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class PathInstance
{
    public enum LoopingType
    {
        PingPong = 0,
        Forward = 1,
        Once = 2,
    }

    public enum ActivationType
    {
        Automatic = 0,
        TouchStart = 1,
    }
    
    public readonly List<Vector2Int> points = new();
    public LoopingType loopingType = LoopingType.PingPong;
    public ActivationType activationType;
}

public class PathingUIManager : MonoBehaviour
{
    [SerializeField] private RectTransform pathingNodePrefab;
    [SerializeField] private RectTransform pathingEdgePrefab;
    [SerializeField] private Transform nodesParent;
    [SerializeField] private Transform edgesParent;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private Changelog changelog;
    [SerializeField] private RectTransform previewNode;
    [SerializeField] private RectTransform previewEdge;
    [SerializeField] private RectTransform pathProperties;
    [SerializeField] private TileEditorState tileEditorState;
    
    private Dictionary<PathInstance, PathUI> paths;
    private int pathPropertiesLayer;

    private void Awake()
    {
        paths = new();
        changelog.ChangeEvent += OnChangeEvent;
        
        HidePreview();
    }

    private void OnDestroy()
    {
        changelog.ChangeEvent -= OnChangeEvent;
    }

    private void OnChangeEvent(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case LayerMetadataChangeInfo { metadataValue: PathInstance pathInstance } layerMetadataChangeInfo:

                if (layerMetadataChangeInfo.type == LayerMetadataChangeInfo.Type.Add)
                {
                    AddPath(pathInstance);
                }
                else
                {
                    RemovePath(pathInstance);
                }
                
                break;
        }
    }

    public void HidePreview()
    {
        previewNode.gameObject.SetActive(false);
        previewEdge.gameObject.SetActive(false);
    }

    public void UpdatePreview(Vector2Int position) => UpdatePreview(position, position);
    public void UpdatePreview(Vector2Int previous, Vector2Int next)
    {
        previewNode.gameObject.SetActive(true);
        previewEdge.gameObject.SetActive(true);
        
        SetNodePosition(next, previewNode);
        SetEdgePosition(previous, next, previewEdge);
    }

    public void AddPath(PathInstance pathInstance)
    {
        if (paths.ContainsKey(pathInstance)) return;
        
        paths.Add(pathInstance, new(pathInstance, this));
    }

    public void RemovePath(PathInstance pathInstance)
    {
        if (!paths.ContainsKey(pathInstance)) return; 
            
        paths[pathInstance].Destroy();
        paths.Remove(pathInstance);
    }
    
    public void ShowPathProperties(int layerID) => pathPropertiesLayer = layerID;
    public void HidePathProperties() => pathPropertiesLayer = -1;

    private void Update()
    {
        foreach (var path in paths.Values)
        {
            path.UpdatePlacement();
        }
        
        pathProperties.gameObject.SetActive(
            pathPropertiesLayer != -1 
            && tileEditorState.LevelInstance.GetLayerMetadata(pathPropertiesLayer).HasValue<PathInstance>());
        
        var layerRect = tileEditorState.LevelInstance.GetLayerRect(pathPropertiesLayer);
        Vector2 offset = spaceUtility.CellToCanvas(layerRect.min - Vector2Int.one, pathProperties);
        Vector2 rectMin = spaceUtility.CellToCanvas(layerRect.min, pathProperties);
        Vector2 rectMax = spaceUtility.CellToCanvas(layerRect.max, pathProperties);
        var layerRectCanvas = new Rect((rectMin + offset) / 2f, rectMax - offset);
        
        pathProperties.position = new(layerRectCanvas.center.x, layerRectCanvas.yMin);
    }

    private void SetNodePosition(Vector2Int position, RectTransform node) =>
        node.position = spaceUtility.CellToCanvas(position, node);

    private void SetEdgePosition(Vector2Int previous, Vector2Int next, RectTransform edge)
    {
        Vector2 previousCanvas = spaceUtility.CellToCanvas(previous, edge);
        Vector2 nextCanvas = spaceUtility.CellToCanvas(next, edge);
        
        edge.position = (previousCanvas + nextCanvas) / 2f;
        edge.right = nextCanvas - previousCanvas;
        edge.sizeDelta = Vector2.right * (nextCanvas - previousCanvas).magnitude;
    }
    
    private class PathUI
    {
        private readonly PathInstance pathInstance;
        private readonly PathingUIManager context;

        private readonly List<RectTransform> nodes;
        private readonly List<RectTransform> edges;
        
        public PathUI(PathInstance pathInstance, PathingUIManager context)
        {
            this.pathInstance = pathInstance;
            this.context = context;

            nodes = new();
            edges = new();
            
            UpdatePlacement();
        }

        public void UpdatePlacement()
        {
            UpdateCount(
                pathInstance.points.Count,
                nodes, 
                context.pathingNodePrefab,
                context.nodesParent);
            UpdateCount(
                Mathf.Max(0, pathInstance.points.Count + (pathInstance.loopingType == PathInstance.LoopingType.Forward ? 0 : -1)),
                edges, 
                context.pathingEdgePrefab, 
                context.edgesParent);
            
            for (int i = 0; i < pathInstance.points.Count; i++)
            {
                context.SetNodePosition(pathInstance.points[i], nodes[i]);
                nodes[i].GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();

                if (i < pathInstance.points.Count - 1 || pathInstance.loopingType == PathInstance.LoopingType.Forward)
                {
                    context.SetEdgePosition(
                        pathInstance.points[i],
                        pathInstance.points[(i + 1) % pathInstance.points.Count],
                        edges[i]);
                }
            }
        }

        private void UpdateCount(int count, List<RectTransform> instances, RectTransform prefab, Transform parent)
        {
            while (instances.Count != count)
            {
                if (instances.Count < count)
                {
                    instances.Add(Instantiate(prefab, parent));
                }
                else
                {
                    var removing = instances[^1];
                    Object.Destroy(removing.gameObject);
                    instances.Remove(removing);
                }
            }
        }

        public void Destroy()
        {
            foreach (var instance in nodes.Concat(edges))
            {
                Object.Destroy(instance.gameObject);
            }
        }
    }
}
