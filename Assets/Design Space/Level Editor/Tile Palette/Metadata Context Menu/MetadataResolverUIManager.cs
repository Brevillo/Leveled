using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class MetadataResolverUIManager : MonoBehaviour
{
    [SerializeField] private Transform entriesParent;
    [SerializeField] private MetadataResolverUISection sectionPrefab;
    [SerializeField] private TileEditorState tileEditorState;
    [SerializeField] private string allSelectedSectionName;
    [SerializeField] private GameObject content;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private RectTransform windowPivot;
    [SerializeField] private GameObject noPropertiesContent;
    [SerializeField] private EditorAction openMetadataEditor;
    [SerializeField] private ToolbarBlackboard toolbarBlackboard;
    [SerializeField] private CanvasGroup unselectedDarkening;
    [SerializeField] private float unselectedDarkeningPercent;

    private List<MetadataResolverUISection> sectionInstances;
    private Vector2 cellPivotWorld;

    private (Vector2Int position, TileData tile)[] ResolvableTilePositions(Vector2Int[] selection) => selection
        .Select(position => (position, tile: tileEditorState.LevelInstance.GetTileOnAnyLayer(position)))
        .Where(item => !item.tile.IsEmpty && item.tile.gameTile.MetadataResolvers.Count > 0)
        .ToArray();

    private void Awake()
    {
        content.SetActive(false);
        sectionInstances = new();
        
        toolbarBlackboard.selection.ChangeEvent += OnSelectionChanged;
    }

    private void OnDestroy()
    {
        toolbarBlackboard.selection.ChangeEvent -= OnSelectionChanged;
    }

    private void OnSelectionChanged(ChangeInfo changeInfo)
    {
        if (changeInfo is ValueChangeInfo<RectInt> selectionChangeInfo
            && selectionChangeInfo.name == toolbarBlackboard.selection.ChangelogName)
        {
            openMetadataEditor.Enabled = ResolvableTilePositions(toolbarBlackboard.SelectionPositions.ToArray()).Any();
        }
    }

    public void Open(Vector2Int[] selection)
    {
        var selectionRectInt = toolbarBlackboard.selection.Value;

        cellPivotWorld = selectionRectInt != default
            ? (spaceUtility.CellToWorld(selectionRectInt.max - Vector2Int.one) +
               spaceUtility.CellToWorld(new(selectionRectInt.xMax - 1, selectionRectInt.yMin))) / 2f
            : spaceUtility.MouseCellCenterWorld;
        
        ClearSections();
        
        // Find all positions with resolvable tiles
        var resolvableTilePositions = ResolvableTilePositions(selection);
        
        content.SetActive(true);

        if (resolvableTilePositions.Length == 0)
        {
            noPropertiesContent.SetActive(true);
            return;
        }
        
        noPropertiesContent.SetActive(false);

        // Get all GameTiles
        var resolvableTiles = resolvableTilePositions
            .GroupBy(item => item.tile.gameTile)
            .Select(group => group.Key)
            .ToArray();

        // Get all MetadataResolvers that are common between all GameTiles
        var commonResolvers = new HashSet<MetadataResolver>(resolvableTiles.First().MetadataResolvers);

        foreach (var tile in resolvableTiles)
        {
            commonResolvers.RemoveWhere(resolver => !tile.MetadataResolvers.Contains(resolver));
        }
        
        // If there are multiple resolvable tile types, create a section for shared resolvers
        if (resolvableTiles.Length > 1 && commonResolvers.Count > 0)
        {
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

    public void Close(BaseEventData baseEventData)
    {
        var pointerEventData = (ExtendedPointerEventData)baseEventData;
        
        if (pointerEventData.button is PointerEventData.InputButton.Left or PointerEventData.InputButton.Right)
        {
            Close();
        }
    }
    
    public void Close()
    {
        content.SetActive(false);

        ClearSections();
    }

    private void Update()
    {
        if (content.activeSelf)
        {
            windowPivot.position = spaceUtility.WorldToCanvas(
                cellPivotWorld + Vector2.right * spaceUtility.Grid.cellSize / 2f,
                windowPivot);

            unselectedDarkening.alpha = unselectedDarkeningPercent;
        }
        else
        {
            unselectedDarkening.alpha = 0f;
        }
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