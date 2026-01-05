using System;
using System.Collections.Generic;
using System.Linq;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using TMPro;
using UnityEngine;

public class LinkingGroupLabelsManager : MonoBehaviour
{
    [SerializeField] private GameObjectPool popupPool;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private Transform labelsParent;
    [SerializeField] private ChangeloggedBool showLinkingGroups;
    [SerializeField] private Changelog changelog;
    
    private readonly HashSet<Vector2Int> linkableTiles = new();
    private readonly Dictionary<Vector2Int, Label> labels = new();

    private readonly struct Label
    {
        public readonly Poolable poolable;
        public readonly TextMeshProUGUI textMesh;
        public readonly RectTransform transform;

        public Label(LinkingGroupLabelsManager context)
        {
            poolable = context.popupPool.Retrieve();
            textMesh = poolable.GetComponentInChildren<TextMeshProUGUI>();
            transform = (RectTransform)poolable.transform;
            transform.SetParent(context.labelsParent);
        }
    }

    private void Start()
    {
        linkableTiles.Clear();
        labels.Clear();
        popupPool.Clear();
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
                
                for (int i = 0; i < tileChangeInfo.positions.Length; i++)
                {
                    Vector2Int position = tileChangeInfo.positions[i];
                    var newTile = tileChangeInfo.newTiles[i];
                    
                    // remove empty tiles
                    if (newTile.IsEmpty)
                    {
                        linkableTiles.Remove(position);
                    }
                    
                    // add linkable tiles
                    else if (newTile.Linkable)
                    {
                        linkableTiles.Add(position);
                    }
                }

                break;
        }
    }

    private void LateUpdate()
    {
        var windowRect = new Rect
        {
            size = Vector2.one * 1.1f,
            center = Vector2.one / 2f,
        };
        
        foreach (var position in linkableTiles)
        {
            if (!labels.ContainsKey(position)
                && windowRect.Contains((Vector2)spaceUtility.CellToWindow(position)))
            {
                labels.Add(position, new(this));
            }
        }

        foreach (var position in labels.Keys.ToArray())
        {
            if (!windowRect.Contains(spaceUtility.CellToWindow(position)) || !linkableTiles.Contains(position))
            {
                labels[position].poolable.Return();
                labels.Remove(position);
            }
        }
        
        foreach (var label in labels)
        {
            var tileData = editorState.Level.GetTile(label.Key);
            
            label.Value.textMesh.text = tileData.linkingGroup;

            Vector3 windowPoint = spaceUtility.WorldToWindow(spaceUtility.CellToWorld(label.Key) + Vector3.down * 0.5f);
            label.Value.transform.position = spaceUtility.GetCanvas(label.Value.transform).pixelRect.size * windowPoint;
            
            label.Value.transform.gameObject.SetActive(showLinkingGroups.Value);
        }
    }
}
