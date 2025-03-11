using UnityEngine;
using static ToolbarActionsManager;

public abstract class ToolbarAction : ScriptableObject
{
    protected const string CreateMenuPath = "Leveled/Toolbar Actions/";
    protected const int CreateMenuOrder = 100;
    
    [SerializeField] private bool defaultHoverSelectionActive = true;
    [SerializeField] protected string changelogMessage;
    
    protected Blackboard blackboard;

    protected SpaceUtility SpaceUtility => blackboard.spaceUtility;
    protected TileEditorState EditorState => blackboard.editorState;
    protected TilePlacer TilePlacer => blackboard.tilePlacer;
    protected LinkingGroupSetter LinkingGroupSetter => blackboard.linkingGroupSetter;

    protected BoundsInt GetCurrentSelection()
    {
        Vector2Int dragEnd = SpaceUtility.MouseCell;
        Vector2Int min = Vector2Int.Min(dragStart, dragEnd);
        Vector2Int max = Vector2Int.Max(dragStart, dragEnd);

        return new((Vector3Int)min, (Vector3Int)(max - min) + Vector3Int.one);
    }
    
    protected ToolSide activeToolSide;
    protected Vector2Int dragStart;

    protected Vector3 SelectionCenter => (SpaceUtility.MouseCellCenterWorld + SpaceUtility.CellToWorld(dragStart)) / 2f;
    protected Vector2 SelectionSize => new Vector2(
        Mathf.Abs(SpaceUtility.MouseCell.x - dragStart.x) + 1,
        Mathf.Abs(SpaceUtility.MouseCell.y - dragStart.y) + 1);

    public void InjectReferences(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }
    
    public void Activate()
    {
        blackboard.selectionOutlineActive = false;
        blackboard.hoverSelectionActive = defaultHoverSelectionActive;
        
        OnActivated();
    }

    public void Deactivate()
    {
        OnDeactivated();
    }
    
    public void InputDown(ToolSide toolSide)
    {
        activeToolSide = toolSide;
        dragStart = SpaceUtility.MouseCell;

        OnDown();
    }

    public void InputPressed(ToolSide toolSide)
    {
        if (activeToolSide != toolSide) return;
        
        OnPressed();
    }

    public void InputReleased(ToolSide toolSide)
    {
        if (activeToolSide != toolSide) return;
        
        OnReleased();

        activeToolSide = ToolSide.None;
    }

    public void Update()
    {
        blackboard.hoverSelectionSize = Vector2.one;
        blackboard.hoverSelectionCenter = SpaceUtility.MouseCellCenterWorld;

        OnUpdate();
    }
    
    protected virtual void OnActivated() { }
    protected virtual void OnDeactivated() { }
    protected virtual void OnDown() { }
    protected virtual void OnPressed() { }
    protected virtual void OnReleased() { }
    protected virtual void OnUpdate() { }
}
