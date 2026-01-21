using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = CreateMenuPath + "Rect Brush")]
public class RectBrushToolAction : ToolbarAction
{
    [SerializeField] private SoundEffect placedSound;

    protected override void OnPressed()
    {
        blackboard.hoverSelection = CurrentSelection;
    }

    protected override void OnReleased()
    {
        placedSound.Play();

        Vector2Int dragEnd = SpaceUtility.MouseCell;
        Vector2Int min = Vector2Int.Min(dragStart, dragEnd);
        Vector2Int max = Vector2Int.Max(dragStart, dragEnd);
        Vector2Int size = max - min + Vector2Int.one;

        var positions = Enumerable.Range(0, size.x * size.y)
            .Select(i => new Vector2Int(min.x + i % size.x, min.y + i / size.x))
            .ToArray();

        var tiles = new TileData[positions.Length];
        Array.Fill(tiles, new(DrawingTile));

        var nullTiles = new TileData[tiles.Length];
        
        foreach (var layer in EditorState.LevelInstance.AllLayerIDs)
        {
            EditorState.SetTiles(positions, layer == 0 ? tiles : nullTiles, changelogMessage, layer);
        }

        if (DrawingTile.MetadataResolvers.Count > 0)
        {
            blackboard.metadataResolverUIManagerReference.value.Open(positions);
        }
    }
}
