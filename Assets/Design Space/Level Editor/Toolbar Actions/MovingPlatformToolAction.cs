using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = CreateMenuPath + "Moving Platform")]
public class MovingPlatformToolAction : ToolbarAction
{
    [SerializeField] private float layerSelectionRadius;
    [SerializeField] private LayerMask layerSelectionMask;
    
    private State state;
    
    private enum State
    {
        None,
        Selecting,
    }

    protected override void OnActivated()
    {
        state = State.None;
    }

    protected override void OnDeactivated()
    {
        TilePlacer.SetSelectedLayer(-1);
    }

    protected override void OnDown()
    {
        switch (state)
        {
            case State.None:

                state = State.Selecting;
                
                break;
        }
    }

    protected override void OnUpdate()
    {
        int mouseLayer = EditorState.LevelInstance.GetLayerIDAt(SpaceUtility.MouseCell);
        TilePlacer.SetSelectedLayer(-1);

        switch (state)
        {
            case State.Selecting:

                blackboard.hoverSelection = CurrentSelection;
                
                break;
            
            default:

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
        }
    }
}