using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = CreateMenuPath + "Moving Platform")]
public class MovingPlatformToolAction : ToolbarAction
{
    private State state;
    private PathInstance drawingPath;

    private enum State
    {
        None,
        Selecting,
        StartingPathDrawing,
        DrawingPath,
    }

    protected override void OnActivated()
    {
        state = State.None;
    }

    protected override void OnDeactivated()
    {
        TilePlacer.SetSelectedLayer(-1);
        PathingUIManager.HidePreview();
    }

    protected override void OnDown()
    {
        switch (state)
        {
            case State.None:

                int mouseLayer = MouseLayer;
                if (mouseLayer > 0 && !EditorState.LevelInstance.GetLayerMetadata(mouseLayer).HasValue<PathInstance>())
                {
                    state = State.StartingPathDrawing;
                }
                else
                {
                    state = State.Selecting;
                }

                break;
            
            case State.DrawingPath:
                
                switch (activeToolSide)
                {
                    case ToolSide.Secondary:

                        CompletePath();

                        break;
                }

                break;
        }
    }
    
    protected override void OnUpdate()
    {
        int mouseLayer = MouseLayer;
        TilePlacer.SetSelectedLayer(-1);

        switch (state)
        {
            case State.Selecting:

                blackboard.hoverSelection = CurrentSelection;
                blackboard.editingLayer = -1;
                
                break;
            
            case State.DrawingPath:
                
                PathingUIManager.UpdatePreview(drawingPath.points[^1], SpaceUtility.MouseCell);
                
                break;
            
            case State.None:
            case State.StartingPathDrawing:

                if (mouseLayer > 0
                    && UIUtility.PointerOverUILayer != UILayer.Default)
                {
                    TilePlacer.SetSelectedLayer(mouseLayer);

                    if (!EditorState.LevelInstance.GetLayerMetadata(mouseLayer).HasValue<PathInstance>())
                    {
                        PathingUIManager.UpdatePreview(SpaceUtility.MouseCell);
                    }
                    
                    var layerRect = EditorState.LevelInstance.GetLayerRect(mouseLayer);
                    layerRect.size += Vector2Int.one;
                    blackboard.hoverSelection = layerRect;
                }
                else
                {
                    PathingUIManager.HidePreview();
                }

                blackboard.editingLayer = mouseLayer > 0 ? mouseLayer : -1;

                break;
        }
        
        PathingUIManager.ShowPathProperties(state == State.None && mouseLayer > 0
            ? mouseLayer
            : -1);
    }

    protected override void OnReleased()
    {
        switch (state)
        {
            case State.Selecting:

                blackboard.selection.Value = blackboard.hoverSelection;
                EditorState.MoveTilesToLayer(blackboard.SelectionPositions.ToArray(),
                    EditorState.LevelInstance.GetNewLayerID());
                blackboard.selection.Value = default;

                state = State.None;

                break;

            case State.StartingPathDrawing:

                int mouseLayer = MouseLayer;

                if (mouseLayer == blackboard.editingLayer)
                {
                    state = State.DrawingPath;
                    
                    drawingPath = new();
                    PathingUIManager.AddPath(drawingPath);
                    drawingPath.points.Add(SpaceUtility.MouseCell);
                }
                else
                {
                    state = State.None;
                }
                
                break;
            
            case State.DrawingPath:

                if (activeToolSide == ToolSide.Primary)
                {
                    var mouseCell = SpaceUtility.MouseCell;

                    if (drawingPath.points.Contains(mouseCell))
                    {
                        drawingPath.pathingType = PathInstance.PathingType.Forward;
                        CompletePath();
                    }
                    else
                    {
                        drawingPath.points.Add(mouseCell);
                    }
                }
                        
                break;
        }
    }
    
    private void CompletePath()
    {
        if (drawingPath.points.Count < 2)
        {
            PathingUIManager.RemovePath(drawingPath);
        }
        else
        {
            blackboard.changelog.StartChangeBundle("New movement path");

            DeletePathOnEditingLayer();

            blackboard.changelog.SendChange(new LayerMetadataChangeInfo(
                "Created movement path",
                blackboard.editingLayer,
                drawingPath,
                LayerMetadataChangeInfo.Type.Add));
            
            blackboard.changelog.EndChangeBundle();
        }
        
        drawingPath = null;
        blackboard.editingLayer = -1;

        state = State.None;
                        
        PathingUIManager.HidePreview();
    }

    public void DeletePathOnEditingLayer()
    {
        if (EditorState.LevelInstance.GetLayerMetadata(blackboard.editingLayer)
            .TryGetValue(out PathInstance existingPath))
        {
            blackboard.changelog.SendChange(new LayerMetadataChangeInfo(
                "Removed movement path",
                blackboard.editingLayer,
                existingPath,
                LayerMetadataChangeInfo.Type.Remove));
        }
    }

    public void TogglePathType()
    {
        if (EditorState.LevelInstance.GetLayerMetadata(blackboard.editingLayer)
            .TryGetValue(out PathInstance path))
        {
            blackboard.changelog.SendChange(new LayerPathTypeChangeInfo(
                "Changed path type",
                blackboard.editingLayer,
                path.pathingType,
                path.pathingType.IncrementEnum()));
        }
    }
}