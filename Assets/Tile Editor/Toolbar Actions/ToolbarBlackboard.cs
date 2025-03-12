using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Toolbar Actions/Toolbar Blackboard")]
public class ToolbarBlackboard : ScriptableObject
{
    public SpaceUtility spaceUtility;
    public LinkingGroupSetter linkingGroupSetter;
    public TileEditorState editorState;
    public TilePlacer tilePlacer;

    public bool selectionOutlineActive;
    public BoundsInt selection;
    
    public Vector3 hoverSelectionCenter;
    public Vector2 hoverSelectionSize;
    public bool hoverSelectionActive;
}
