using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LinkingGroupMetadataResolver : MonoBehaviour
{
    [SerializeField] private string[] defaultOptions;
    [SerializeField] private string placeholderText;
    [SerializeField] private MetadataResolverUIStringID stringID;
    [SerializeField] private MetadataResolverUI metadataResolverUI;
    [SerializeField] private TileEditorState tileEditorState;

    private string[] Options => defaultOptions
        .Concat(tileEditorState.Level
            .GetAllMetadata<LinkingGroup>()
            .Select(group => group.groupID))
        .Where(group => !string.IsNullOrEmpty(group))
        .GroupBy(groupID => groupID)
        .Select(group => group.Key)
        .ToArray();
    
    private void Awake()
    {
        stringID.OptionSelected += OnOptionSelected;
        metadataResolverUI.Initialized += OnInitialized;
    }

    private void OnInitialized(Vector2Int[] selection)
    {
        var selectedGroupIDs = selection
            .GroupBy(position => tileEditorState.Level.GetTile(position).GetMetaData<LinkingGroup>().groupID)
            .Select(group => group.Key)
            .Where(groupID => !string.IsNullOrEmpty(groupID))
            .ToArray();

        string selected = selectedGroupIDs.Length == 1
            ? selectedGroupIDs.First()
            : "-";
        
        stringID.Initialize(selected, placeholderText, Options);
    }

    private void OnOptionSelected(string option)
    {
        metadataResolverUI.UpdateMetadata(new LinkingGroup(option));
    }
}