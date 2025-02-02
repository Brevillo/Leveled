using System;
using System.Collections.Generic;
using System.Linq;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using TMPro;
using UnityEngine;

public class LinkingGroupLabelsManager : MonoBehaviour
{
    [SerializeField] private CanvasGameObjectPool popupPool;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private Transform labelsParent;

    private Dictionary<Vector3Int, Label> labels;

    private readonly struct Label
    {
        public readonly Poolable poolable;
        public readonly TextMeshProUGUI textMesh;
        public readonly RectTransform transform;

        public Label(Poolable poolable)
        {
            this.poolable = poolable;
            textMesh = poolable.GetComponentInChildren<TextMeshProUGUI>();
            transform = poolable.transform as RectTransform;
        }
    }
    
    private void Awake()
    {
        labels = new();
    }

    private void Start()
    {
        popupPool.Clear();
        popupPool.SetParent(labelsParent);
    }

    private void OnEnable()
    {
        editorState.EditorChanged += OnEditorChanged;
    }

    private void OnDisable()
    {
        editorState.EditorChanged -= OnEditorChanged;
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case ShowLinkingGroupsChangeInfo showLinkingGroupsChangeInfo:

                if (showLinkingGroupsChangeInfo.newValue && !showLinkingGroupsChangeInfo.previousValue)
                {
                    CreateLabels();
                }
                else
                {
                    ClearLabels();
                }
                
                break;
                
            case MultiTileChangeInfo multiTileChangeInfo:
                
                if (!editorState.ShowLinkingGroups) break;

                for (int i = 0; i < multiTileChangeInfo.positions.Length; i++)
                {
                    Vector3Int position = multiTileChangeInfo.positions[i];
                    var newTile = multiTileChangeInfo.newTiles[i];
                    
                    // remove empty tiles
                    if (newTile.IsEmpty && labels.TryGetValue(position, out var label))
                    {
                        label.poolable.Return();
                        labels.Remove(position);
                    }
                    
                    // create filled tiles
                    else if (newTile.Linkable && !labels.ContainsKey(position))
                    {
                        labels.Add(position, new(popupPool.Retrieve()));
                    }
                }

                break;
        }
    }

    private void CreateLabels()
    {
        var tiles = editorState.LinkedTiles;

        foreach (var (position, _) in tiles)
        {
            labels.Add(position, new(popupPool.Retrieve()));
        }
    }

    private void ClearLabels()
    {
        foreach (var label in labels.Values)
        {
            label.poolable.Return();
        }
        
        labels.Clear();
    }

    private void LateUpdate()
    {
        foreach (var label in labels)
        {
            var tileData = editorState.GetTile(label.Key);
            
            label.Value.textMesh.text = tileData.linkingGroup;

            Vector3 windowPoint = spaceUtility.WorldToWindow(spaceUtility.CellToWorld(label.Key));
            label.Value.transform.position = spaceUtility.GetCanvas(label.Value.transform).pixelRect.size * windowPoint;
        }
    }
}
