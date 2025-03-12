using System;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarActionsUI : MonoBehaviour
{
    [Header("Hover Selection")]
    [SerializeField] private Image selectionOutline;
    [SerializeField] private Image hoverSelection;
    [SerializeField] private float hoverSelectionSpeed;
    [Header("References")]
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private ToolbarBlackboard blackboard;
    
    private Vector3 hoverSelectionPositionVelocity;
    private Vector2 hoverSelectionSizeVelocity;
    
    private void OnEnable()
    {
        gameStateManager.GameStateChanged += OnGameStateChanged;
    }
    
    private void OnDisable()
    {
        gameStateManager.GameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState gameState)
    {
        selectionOutline.gameObject.SetActive(false);
    }
    
    private void Update()
    {
        // Selection outline
        var selectionOutlineTransform = selectionOutline.rectTransform;
        Vector2 selectionOutlineSize = spaceUtility.CellToCanvas((Vector2Int)blackboard.selection.max, selectionOutlineTransform) -
                                       spaceUtility.CellToCanvas((Vector2Int)blackboard.selection.min, selectionOutlineTransform);
        selectionOutlineTransform.sizeDelta = selectionOutlineSize;
        selectionOutlineTransform.position = spaceUtility.WorldToCanvas(
            spaceUtility.GetBoundsIntCenterWorld(blackboard.selection),
            selectionOutlineTransform);
        
        selectionOutline.gameObject.SetActive(blackboard.selectionOutlineActive);

        // Hover Selection
        Vector3 hoverCenter = blackboard.hoverSelectionCenter;
        Vector2 hoverSize = blackboard.hoverSelectionSize;

        var hoverSelectionTransform = hoverSelection.rectTransform;
        hoverSize =
            spaceUtility.WorldToCanvas((Vector2)hoverCenter + hoverSize / 2f,
                hoverSelectionTransform) -
            spaceUtility.WorldToCanvas((Vector2)hoverCenter - hoverSize / 2f,
                hoverSelectionTransform);
        hoverCenter = spaceUtility.WorldToCanvas(hoverCenter, hoverSelectionTransform);
        
        bool hoverSelectionActive =
            Cursor.visible &&
            !editorState.PointerOverUI &&
            blackboard.hoverSelectionActive;
        
        hoverSelection.gameObject.SetActive(hoverSelectionActive);

        hoverSelectionTransform.sizeDelta = hoverSelectionActive
            ? Vector2.SmoothDamp(hoverSelectionTransform.sizeDelta, hoverSize, ref hoverSelectionSizeVelocity,
                hoverSelectionSpeed)
            : hoverSize;

        hoverSelection.transform.position = hoverSelectionActive
            ? Vector3.SmoothDamp(hoverSelection.transform.position, hoverCenter,
                ref hoverSelectionPositionVelocity, hoverSelectionSpeed)
            : hoverCenter;
    }
}
