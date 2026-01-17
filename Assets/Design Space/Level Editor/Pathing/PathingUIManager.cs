using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class PathInstance
{
    public enum PathingType
    {
        Forward,
        PingPong,
        Once,
    }
    
    public readonly List<Vector2Int> points = new();
    public PathingType pathingType;
}

public class PathingUIManager : MonoBehaviour
{
    [SerializeField] private RectTransform pathingNodePrefab;
    [SerializeField] private RectTransform pathingEdgePrefab;
    [SerializeField] private Transform nodesParent;
    [SerializeField] private Transform edgesParent;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private Changelog changelog;
    
    private Dictionary<PathInstance, PathUI> paths;

    private void Awake()
    {
        paths = new();
        changelog.ChangeEvent += OnChangeEvent;
    }

    private void OnDestroy()
    {
        changelog.ChangeEvent -= OnChangeEvent;
    }

    private void OnChangeEvent(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case LayerMetadataChangeInfo layerMetadataChangeInfo 
                when layerMetadataChangeInfo.metadataValue is PathInstance pathInstance:

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
    
    private void Update()
    {
        foreach (var path in paths.Values)
        {
            path.UpdatePlacement();
        }
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
                Mathf.Max(0, pathInstance.points.Count + (pathInstance.pathingType == PathInstance.PathingType.Forward ? 0 : -1)),
                edges, 
                context.pathingEdgePrefab, 
                context.edgesParent);
            
            for (int i = 0; i < pathInstance.points.Count; i++)
            {
                Vector2 point = context.spaceUtility.CellToCanvas(pathInstance.points[i], nodes[i]);
                Vector2 next =
                    context.spaceUtility.CellToCanvas(
                        pathInstance.points[(i + 1) % pathInstance.points.Count], nodes[i]);
                
                nodes[i].position = point;
                nodes[i].GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();

                if (i < pathInstance.points.Count - 1 || pathInstance.pathingType == PathInstance.PathingType.Forward)
                {
                    edges[i].position = (point + next) / 2f;
                    edges[i].right = next - point;
                    edges[i].sizeDelta = Vector2.right * (next - point).magnitude;
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
