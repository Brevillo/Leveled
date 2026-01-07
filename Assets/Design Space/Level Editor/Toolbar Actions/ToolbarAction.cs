using UnityEngine;

public abstract class ToolbarAction : ScriptableObject
{
    protected const string CreateMenuPath = ProjectConstants.ToolbarActionsFolder + "Action ";
    
    [SerializeField] private bool defaultHoverSelectionActive = true;
    [SerializeField] protected string changelogMessage;
    [SerializeField] private string toolName;
    
    protected ToolbarBlackboard blackboard;

    protected SpaceUtility SpaceUtility => blackboard.spaceUtility;
    protected TileEditorState EditorState => blackboard.editorState;
    protected TilePlacer TilePlacer => blackboard.tilePlacer;
    protected LinkingGroupSetter LinkingGroupSetter => blackboard.linkingGroupSetter;
    public string ToolName => toolName;
    
    protected ToolSide activeToolSide;
    protected Vector2Int dragStart;

    protected RectInt CurrentSelection
    {
        get
        {
            Vector2Int current = SpaceUtility.MouseCell;

            Vector2Int min = Vector2Int.Min(current, dragStart);
            Vector2Int max = Vector2Int.Max(current, dragStart);
            
            return new(min, max - min + Vector2Int.one);
        }
    }
    
    protected virtual GameTile DrawingTile => activeToolSide switch
    {
        ToolSide.Primary => blackboard.primaryTile.Value,
        ToolSide.Secondary => blackboard.secondaryTile.Value,
        _ => null,
    };
    
    public void InjectReferences(ToolbarBlackboard blackboard)
    {
        this.blackboard = blackboard;
    }
    
    public void Activate()
    {
        blackboard.hoverSelectionActive = defaultHoverSelectionActive;

        OnActivated();
    }

    public void Deactivate()
    {
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
        blackboard.hoverSelection = new(SpaceUtility.MouseCell, Vector2Int.one);

        OnUpdate();
    }
    
    protected virtual void OnDown() { }
    protected virtual void OnPressed() { }
    protected virtual void OnReleased() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnActivated() { }
}
