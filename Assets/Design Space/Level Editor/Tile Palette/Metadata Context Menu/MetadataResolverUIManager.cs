using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MetadataResolverUIManager : MonoBehaviour
{
    [SerializeField] private Transform entriesParent;
    [SerializeField] private MetadataResolverUISection sectionPrefab;
    [SerializeField] private TileEditorState tileEditorState;
    [SerializeField] private string allSelectedSectionName;
    [SerializeField] private GameObject content;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private RectTransform windowPivot;

    private List<MetadataResolverUISection> sectionInstances;

    private void Awake()
    {
        content.SetActive(false);
        sectionInstances = new();
    } 

    public void Open(Vector2Int[] selection)
    {
        windowPivot.position = spaceUtility.MouseCanvas(windowPivot);
        
        ClearSections();
        
        // Find all positions with resolvable tiles
        var resolvableTilePositions = selection
            .Select(position => (position, tile: tileEditorState.Level.GetTile(position)))
            .Where(item => !item.tile.IsEmpty && item.tile.gameTile.MetadataResolvers.Count > 0)
            .ToArray();

        if (resolvableTilePositions.Length == 0)
        {
            return;
        }
        
        content.SetActive(true);

        // Get all GameTiles
        var resolvableTiles = resolvableTilePositions
            .GroupBy(item => item.tile.gameTile)
            .Select(group => group.Key)
            .ToArray();

        // If there are multiple resolvable tile types, create a section for shared resolvers
        if (resolvableTiles.Length > 1)
        {
            // Get all MetadataResolvers that are common between all GameTiles
            var commonResolvers = new HashSet<MetadataResolver>(resolvableTiles.First().MetadataResolvers);

            foreach (var tile in resolvableTiles)
            {
                commonResolvers.RemoveWhere(resolver => !tile.MetadataResolvers.Contains(resolver));
            }
            
            // Get position of all tiles that contain at least one of the common MetadataResolvers
            var commonResolverPositions = resolvableTilePositions
                .Where(item => commonResolvers.Overlaps(item.tile.gameTile.MetadataResolvers))
                .Select(item => item.position)
                .ToArray();
            
            // Create section for shared resolvers
            var commonSection = Instantiate(sectionPrefab, entriesParent);
            
            commonSection.Initialize(
                allSelectedSectionName,
                commonResolvers.ToList(),
                commonResolverPositions);

            sectionInstances.Add(commonSection);
        }
        
        // Create sections for each type of selected GameTile
        foreach (var tile in resolvableTiles)
        {
            var newSection = Instantiate(sectionPrefab, entriesParent);
           
            newSection.Initialize(
                tile.Tooltip,
                tile.MetadataResolvers,
                resolvableTilePositions
                    .Where(item => item.tile.gameTile == tile)
                    .Select(item => item.position)
                    .ToArray());
            
            sectionInstances.Add(newSection);
        }
    }

    public void Close()
    {
        content.SetActive(false);

        ClearSections();
    }

    private void ClearSections()
    {
        foreach (var section in sectionInstances)
        {
            Destroy(section.gameObject);
        }

        sectionInstances.Clear();
    }
}