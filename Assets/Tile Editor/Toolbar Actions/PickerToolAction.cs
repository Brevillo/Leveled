using UnityEngine;

[CreateAssetMenu(menuName = CreateMenuPath + "Picker", order = CreateMenuOrder)]
public class PickerToolAction : ToolbarAction
{
    protected override void OnReleased()
    {
        var gameTile = EditorState.GetTile(SpaceUtility.MouseCell).gameTile;
        
        switch (activeToolSide)
        {
            case ToolSide.Primary:
                EditorState.PrimaryTile = gameTile;
                break;
            
            case ToolSide.Secondary:
                EditorState.SecondaryTile = gameTile;
                break;
        }
    }
}
