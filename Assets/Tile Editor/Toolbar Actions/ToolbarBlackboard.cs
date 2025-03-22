using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Toolbar Actions/Toolbar Blackboard", order = 0)]
public class ToolbarBlackboard : ScriptableObject
{
    [SerializeField] private string deselectionChangelogMessage;

    public TileEditorState editorState;
    public SpaceUtility spaceUtility;
    public TilePlacer tilePlacer;
    public LinkingGroupSetter linkingGroupSetter;
    
    public BoundsInt selection;
    
    public BoundsInt hoverSelection;
    public bool hoverSelectionActive;
    public bool snapHoverSelection;

    public void ResetValues()
    {
        selection = default;

        hoverSelection = default;
        hoverSelectionActive = false;
        snapHoverSelection = false;
    }

    public void SetSelection(BoundsInt value, string description)
    {
        editorState.SendEditorChange(new AreaSelectionChangeInfo(selection, value, description));
    }

    public void Deselect()
    {
        if (selection == default) return;
        
        SetSelection(default, deselectionChangelogMessage);
    }
}
