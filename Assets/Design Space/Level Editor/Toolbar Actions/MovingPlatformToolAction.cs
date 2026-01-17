using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = CreateMenuPath + "Moving Platform")]
public class MovingPlatformToolAction : ToolbarAction
{
    private State state;
    private PathInstance drawingPath;
    private int pathDrawingLayer;
    
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
        if (state == State.DrawingPath)
        {
            
        }
        
        TilePlacer.SetSelectedLayer(-1);
    }

    protected override void OnDown()
    {
        switch (state)
        {
            case State.None:

                int mouseLayer = MouseLayer;
                if (mouseLayer > 0)
                {
                    state = State.StartingPathDrawing;
                    pathDrawingLayer = mouseLayer;
                }
                else
                {
                    state = State.Selecting;
                }

                break;
            
            case State.DrawingPath:

                switch (activeToolSide)
                {
                    case ToolSide.Primary:

                        drawingPath.points.Add(SpaceUtility.MouseCell);
                        
                        break;
                    
                    case ToolSide.Secondary:
                        
                        blackboard.changelog.StartChangeBundle("New movement path");
                        
                        var existingPath = EditorState.LevelInstance.GetLayerMetadata(pathDrawingLayer)
                            .GetValueOrDefault<PathInstance>();
                    
                        if (existingPath != null)
                        {
                            EditorState.ModifyLayerMetadata(
                                pathDrawingLayer,
                                existingPath,
                                "Removed movement path",
                                LayerMetadataChangeInfo.Type.Remove);
                        } 

                        EditorState.ModifyLayerMetadata(pathDrawingLayer, drawingPath, "Created movement path", LayerMetadataChangeInfo.Type.Add);
                        
                        blackboard.changelog.EndChangeBundle();
                        
                        drawingPath = null;
                        pathDrawingLayer = -1;

                        state = State.None;
                        
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
                
                break;
            
            case State.DrawingPath:

                drawingPath.points[^1] = SpaceUtility.MouseCell;
                
                break;
            
            case State.None:
            case State.StartingPathDrawing:

                if (mouseLayer > 0 && !UIUtility.PointerOverUI)
                {
                    TilePlacer.SetSelectedLayer(mouseLayer);
                    
                    var layerRect = EditorState.LevelInstance.GetLayerRect(mouseLayer);
                    layerRect.size += Vector2Int.one;
                    blackboard.hoverSelection = layerRect;
                }
                
                break;
        }
    }

    protected override void OnReleased()
    {
        switch (state)
        {
            case State.Selecting:

                if (blackboard.hoverSelection.size == Vector2Int.one) break;
                
                blackboard.selection.Value = blackboard.hoverSelection;
                EditorState.MoveTilesToLayer(blackboard.SelectionPositions.ToArray(), EditorState.LevelInstance.GetNewLayerID());
                blackboard.selection.Value = default;
                
                state = State.None;
                
                break;
            
            case State.StartingPathDrawing:

                int mouseLayer = MouseLayer;

                if (mouseLayer == pathDrawingLayer)
                {
                    state = State.DrawingPath;
                    
                    drawingPath = new();
                    blackboard.pathingUIManagerReference.value.AddPath(drawingPath);
                    drawingPath.points.Add(SpaceUtility.MouseCell);
                    drawingPath.points.Add(SpaceUtility.MouseCell);
                }
                else
                {
                    state = State.None;
                }
                
                break;
        }
    }
}