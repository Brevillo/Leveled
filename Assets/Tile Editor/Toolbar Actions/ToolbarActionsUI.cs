using UnityEngine;

public class ToolbarActionsUI : MonoBehaviour
{
    [Header("Hover Selection")]
    [SerializeField] private SelectionUIElement selectionOutline;
    [SerializeField] private SelectionUIElement hoverSelection;
    [SerializeField] private SelectionUIElement tilemapBoundsOutline;
    [Header("References")]
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private ToolbarBlackboard blackboard;
    [SerializeField] private TilePlacer tilePlacer;
    
    private void LateUpdate()
    {
        selectionOutline.SetSize(blackboard.selection);
        selectionOutline.gameObject.SetActive(blackboard.selection != default && gameStateManager.GameState == GameState.Editing);

        hoverSelection.SetSize(blackboard.hoverSelection, blackboard.snapHoverSelection);
        blackboard.snapHoverSelection = false;
        hoverSelection.gameObject.SetActive(//Cursor.visible &&
                                            !editorState.PointerOverUI &&
                                            blackboard.hoverSelectionActive);
        
        tilemapBoundsOutline.SetSize(tilePlacer.BoundsInt);
        tilemapBoundsOutline.gameObject.SetActive(tilePlacer.BoundsInt != default);
    }
}
