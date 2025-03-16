using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Toolbar Actions/Toolbar Blackboard", order = 0)]
public class ToolbarBlackboard : ScriptableObject
{
    public TileEditorState editorState;
    public SpaceUtility spaceUtility;
    public TilePlacer tilePlacer;
    public LinkingGroupSetter linkingGroupSetter;

    public bool selectionOutlineActive;
    public BoundsInt selection;
    
    public BoundsInt hoverSelection;
    public bool hoverSelectionActive;
    public bool snapHoverSelection;

    public void ResetValues()
    {
        selectionOutlineActive = false;
        selection = default;

        hoverSelection = default;
        hoverSelectionActive = false;
        snapHoverSelection = false;
    }
}
