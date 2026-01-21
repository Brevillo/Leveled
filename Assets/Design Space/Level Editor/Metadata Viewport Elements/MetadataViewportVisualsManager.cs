using System.Collections.Generic;
using System.Linq;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using UnityEngine;

public class MetadataViewportVisualsManager : MonoBehaviour
{
    [SerializeField] private TileEditorState tileEditorState;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private Transform elementsParent;
    [SerializeField] private Changelog changelog;
    [SerializeField] private List<MetadataViewportData> metadataViewportDatas;

    private MetadataCategory[] categories;
    
    private void Awake()
    {
        categories = metadataViewportDatas
            .Select(data => new MetadataCategory(data, this))
            .ToArray();
    }

    private void OnEnable()
    {
        changelog.ChangeEvent += OnChangeEvent;
    }

    private void OnDisable()
    {
        changelog.ChangeEvent -= OnChangeEvent;
    }

    private void OnChangeEvent(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case TileChangeInfo tileChangeInfo:

                foreach (var category in categories)
                {
                    category.UpdateManagedTiles(tileChangeInfo);
                }
                
                break;
        }
    }
    
    private void LateUpdate()
    {
        foreach (var category in categories)
        {
            category.UpdateVisualPositions();
        }
    }
    
    private class MetadataCategory
    {
        private readonly MetadataViewportData data;
        private readonly MetadataViewportVisualsManager context;
        private readonly Transform elementsParent;
        
        private readonly ObjectPool<Poolable> visualsPool = new();
        
        // Using Vector3Int to have .z as layerID
        private readonly HashSet<Vector3Int> metadataPositions = new();
        private readonly Dictionary<Vector3Int, Element> elements = new();
        
        private Poolable GetPoolable()
        {
            var poolable = visualsPool.Retrieve(NewVisual);
            poolable.Returned += OnReturned;
            
            poolable.gameObject.SetActive(true);
            
            return poolable;
        }

        private static void OnReturned(Poolable poolable)
        {
            poolable.gameObject.SetActive(false);

            poolable.Returned -= OnReturned;
        }

        private Poolable NewVisual() =>
            Instantiate(data.MetadataViewportVisualPrefab, elementsParent).GetComponent<Poolable>();

        private readonly struct Element
        {
            public readonly Poolable poolable;
            public readonly MetadataViewportVisual visual;
            public readonly RectTransform transform;

            public Element(MetadataCategory context)
            {
                poolable = context.GetPoolable();
                visual = poolable.GetComponent<MetadataViewportVisual>();
                transform = (RectTransform)poolable.transform;
                transform.SetParent(context.elementsParent);
            }
        }

        public MetadataCategory(MetadataViewportData data, MetadataViewportVisualsManager context)
        {
            this.data = data;
            this.context = context;

            elementsParent = new GameObject($"{data.name} Metadata Category").transform;
            elementsParent.SetParent(context.elementsParent);
        }

        public void UpdateManagedTiles(TileChangeInfo tileChangeInfo)
        {
            for (int i = 0; i < tileChangeInfo.positions.Length; i++)
            {
                Vector3Int position = new(
                    tileChangeInfo.positions[i].x, tileChangeInfo.positions[i].y,
                    tileChangeInfo.layerID);
                
                var newTile = tileChangeInfo.newTiles[i];

                if (newTile.IsEmpty)
                {
                    metadataPositions.Remove(position);
                }
                else if (newTile.metadata != null && newTile.metadata.HasValue(data.MetadataType))
                {
                    if (metadataPositions.Add(position) &&
                        elements.TryGetValue(position, out var element))
                    {
                        element.visual.UpdateVisual(newTile.metadata.GetValueOrDefault(data.MetadataType));
                    }
                }
            }
        }

        public void UpdateVisualPositions()
        {
            var windowRect = new Rect
            {
                size = Vector2.one * 1.1f,
                center = Vector2.one / 2f,
            };
            
            foreach (var position in metadataPositions)
            {
                if (!elements.ContainsKey(position)
                    && windowRect.Contains((Vector2)context.spaceUtility.CellToWindow((Vector2Int)position)))
                {
                    var element = new Element(this);
                    elements.Add(position, element);

                    element.visual.UpdateVisual(context.tileEditorState.LevelInstance
                        .GetTile((Vector2Int)position, position.z).metadata.GetValueOrDefault(data.MetadataType));
                }
            }

            foreach (var position in elements.Keys.ToArray())
            {
                if (!windowRect.Contains(context.spaceUtility.CellToWindow((Vector2Int)position)) || !metadataPositions.Contains(position))
                {
                    elements[position].poolable.Return();
                    elements.Remove(position);
                }
            }
        
            foreach (var (position, element) in elements)
            {
                Vector3 windowPoint = context.spaceUtility.WorldToWindow(context.spaceUtility.CellToWorld((Vector2Int)position) + Vector3.down * 0.5f);
                element.transform.position = context.spaceUtility.GetCanvas(element.transform).pixelRect.size * windowPoint;
            
                element.transform.gameObject.SetActive(data.ShowVisuals);
            }
        }
    }
}