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
    
    public Vector3 hoverSelectionCenter;
    public Vector2 hoverSelectionSize;
    public bool hoverSelectionActive;
}
