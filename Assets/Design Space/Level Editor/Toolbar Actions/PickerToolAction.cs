using UnityEngine;

[CreateAssetMenu(menuName = CreateMenuPath + "Picker")]
public class PickerToolAction : ToolbarAction
{
    protected override void OnReleased()
    {
        var gameTile = EditorState.Level.GetTile(SpaceUtility.MouseCell).gameTile;
        
        switch (activeToolSide)
        {
            case ToolSide.Primary:
                blackboard.primaryTile.Value = gameTile;
                break;
            
            case ToolSide.Secondary:
                blackboard.secondaryTile.Value = gameTile;
                break;
        }
    }
}
