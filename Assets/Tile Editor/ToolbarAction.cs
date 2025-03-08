using UnityEngine;
using static ToolbarActionsManager;

public abstract class ToolbarAction : ScriptableObject
{
    protected const string CreateMenuPath = "Leveled/Toolbar Actions/";
    protected const int CreateMenuOrder = 100;
    
    [SerializeField] private Sprite icon;
    [SerializeField] private bool defaultHoverSelectionActive = true;
    [SerializeField] private string keymap;
    [SerializeField] private string tooltip;
    [SerializeField] private EditorAction editorAction;
    
    public Sprite IconSprite => icon;
    public string Keymap => editorAction.Keymap;
    public string Tooltip => tooltip;
    public EditorAction EditorAction => editorAction;
    
    protected Blackboard blackboard;

    protected SpaceUtility SpaceUtility => blackboard.spaceUtility;
    protected TileEditorState EditorState => blackboard.editorState;
    protected TilePlacer TilePlacer => blackboard.tilePlacer;
    protected LinkingGroupSetter LinkingGroupSetter => blackboard.linkingGroupSetter;

    protected BoundsInt GetCurrentSelection()
    {
        Vector3Int dragEnd = SpaceUtility.MouseCell;
        Vector3Int min = Vector3Int.Min(dragStart, dragEnd);
        Vector3Int max = Vector3Int.Max(dragStart, dragEnd);

        return new(min, max - min + Vector3Int.one);
    }
    
    protected ToolSide activeToolSide;
    protected Vector3Int dragStart;

    protected Vector3 SelectionCenter => (SpaceUtility.MouseCellCenterWorld + SpaceUtility.CellToWorld(dragStart)) / 2f;
    protected Vector2 SelectionSize => new Vector2(
        Mathf.Abs(SpaceUtility.MouseCell.x - dragStart.x) + 1,
        Mathf.Abs(SpaceUtility.MouseCell.y - dragStart.y) + 1);

    public void Activate(Blackboard blackboard)
    {
        this.blackboard = blackboard;

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
