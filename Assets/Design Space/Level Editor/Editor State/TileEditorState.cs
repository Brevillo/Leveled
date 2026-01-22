using System;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Tile Editor State")]
public class TileEditorState : GameService
{
    [SerializeField] private Changelog changelog;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private TilePlacerReference tilePlacerReference;
    
    private readonly LevelInstance levelInstance = new LevelInstance();

    public void MoveTilesToLayer(Vector2Int[] positions, int layerID)
    {
        changelog.StartChangeBundle("Move tiles to new layer");

        var movingTiles = positions.Select(LevelInstance.GetTileOnAnyLayer).ToArray();
        var nullTiles = new TileData[positions.Length];
        
        foreach (var layer in LevelInstance.AllLayerIDs)
        {
            if (layer == layerID) continue;
            
            SetTiles(
                positions,
                nullTiles,
                $"Changed tiles on layer {layer}",
                layer);
        }
        
        SetTiles(
            positions,
            movingTiles,
            $"Changed tiles on layer {layerID}",
            layerID);
        
        changelog.EndChangeBundle();
    }

    public void SetTiles(Vector2Int[] positions, TileData[] tiles, string description, int layerID = 0) =>
        changelog.SendChange(new TileChangeInfo(
            description,
            layerID, 
            positions,
            positions
                .Select(position => LevelInstance.GetTile(position, layerID))
                .ToArray(), tiles));

    public LevelInstance LevelInstance => levelInstance;

    public Rect WorldBounds => new Rect
    {
        size = LevelInstance.Bounds.size,
        center = spaceUtility.Grid.transform.TransformPoint(LevelInstance.Bounds.center),
    };
    
    protected override void Initialize()
    {
        changelog.StateUpdated += OnStateUpdated;
    }

    protected override void InstanceDestroyed()
    {
        changelog.StateUpdated -= OnStateUpdated;
    }

    private void OnStateUpdated(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case TileChangeInfo tileChangeInfo:

                if (LevelInstance.UpdateLayers(tileChangeInfo.layerID)) return;
                
                for (int i = 0; i < tileChangeInfo.positions.Length; i++)
                {
                   LevelInstance.SetTile(tileChangeInfo.positions[i], tileChangeInfo.newTiles[i], tileChangeInfo.layerID);
                }

                LevelInstance.UpdateBounds();
                
                break;
            
            case LayerMetadataChangeInfo layerMetadataChangeInfo:

                if (LevelInstance.UpdateLayers(layerMetadataChangeInfo.layerID)) return;

                var layerMetadata = LevelInstance.GetLayerMetadata(layerMetadataChangeInfo.layerID);
                
                switch (layerMetadataChangeInfo.type)
                {
                    case LayerMetadataChangeInfo.Type.Add:
                        
                        layerMetadata.SetValue(layerMetadataChangeInfo.metadataValue);
                        
                        break;
                    
                    case LayerMetadataChangeInfo.Type.Remove:

                        layerMetadata.RemoveValue(layerMetadataChangeInfo.metadataValue);
                        
                        break;
                }
                
                break;
            
            case LayeredValueChangeInfo<PathInstance.LoopingType> loopingTypeChangeInfo:

                if (LevelInstance.UpdateLayers(loopingTypeChangeInfo.layerID)) return;

                layerMetadata = LevelInstance.GetLayerMetadata(loopingTypeChangeInfo.layerID);

                if (layerMetadata.TryGetValue<PathInstance>(out var pathInstance))
                {
                    pathInstance.loopingType = loopingTypeChangeInfo.newValue;
                }

                break;
            
            case LayeredValueChangeInfo<PathInstance.ActivationType> activationTypeChangeInfo:

                if (LevelInstance.UpdateLayers(activationTypeChangeInfo.layerID)) return;

                layerMetadata = LevelInstance.GetLayerMetadata(activationTypeChangeInfo.layerID);

                if (layerMetadata.TryGetValue(out pathInstance))
                {
                    pathInstance.activationType = activationTypeChangeInfo.newValue;
                }

                break;
        }
    }
    
    public void ClearAllTiles()
    {
        changelog.SendChange(LevelInstance.ClearAllTilesChangeInfo);
        tilePlacerReference.value.ResetTilemaps();
    }

    public void SetLevelData(LevelData levelData, GameTilePalette palette)
    {
        ClearAllTiles();
        
        changelog.SendChange(LevelInstance.SetLevelData(levelData, palette));
        changelog.ClearChangelog();
    }
}
