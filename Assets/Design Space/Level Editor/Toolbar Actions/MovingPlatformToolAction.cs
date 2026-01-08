using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = CreateMenuPath + "Moving Platform")]
public class MovingPlatformToolAction : ToolbarAction
{
    private State state;
    
    private enum State
    {
        None,
        Selecting,
        gg
    }

    protected override void OnActivated()
    {
        state = State.None;
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
        switch (state)
        {
            case State.Selecting:

                blackboard.hoverSelection = CurrentSelection;
                
                break;
        }
    }

    protected override void OnReleased()
    {
        switch (state)
        {
            case State.Selecting:

                // blackboard.selection.Value = CurrentSelection;
                //
                // var positions = new List<Vector2Int>();
                // foreach (Vector2Int position in blackboard.selection.Value.allPositionsWithin)
                // {
                //     positions.Add(position);
                // }
                //
                // blackboard.changelog.StartChangeBundle("Moved tiles to new layer");
                //
                // EditorState.SetTiles(positions);
                
                break;
        }
    }
}