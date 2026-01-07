using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ToolbarActionsFolder + "Toolbar Blackboard")]
public class ToolbarBlackboard : ScriptableObject
{
    [SerializeField] private string deselectionChangelogMessage;

    public TileEditorState editorState;
    public SpaceUtility spaceUtility;
    public TilePlacer tilePlacer;
    public LinkingGroupSetter linkingGroupSetter;
    public Changelog changelog;

    public ChangeloggedToolbarAction activeTool;
    public ChangeloggedGameTile primaryTile;
    public ChangeloggedGameTile secondaryTile;
    
    public ChangeloggedRectInt selection;
    
    public RectInt hoverSelection;
    public bool hoverSelectionActive;
    public bool snapHoverSelection;

    public void ResetValues()
    {
        hoverSelection = default;
        hoverSelectionActive = false;
        snapHoverSelection = false;
    }

    public void Deselect()
    {
        if (selection.Value == default) return;
        
        selection.Value = default;
    }
}
