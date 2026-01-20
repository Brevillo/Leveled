using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = CreateMenuPath + "Mover")]
public class MoverToolAction : ToolbarAction
{
    private Vector2 previousMousePosition;

    protected override void OnDown()
    {
        previousMousePosition = Input.mousePosition;
        blackboard.hoverSelectionActive = false;
    }

    protected override void OnPressed()
    {
        Vector2 mousePosition = Input.mousePosition;
        
        SpaceUtility.Camera.transform.position -= SpaceUtility.Camera.ScreenToWorldPoint(mousePosition) -
                                                  SpaceUtility.Camera.ScreenToWorldPoint(previousMousePosition);

        previousMousePosition = mousePosition;
    }
    
    protected override void OnReleased()
    {
        blackboard.hoverSelectionActive = true;
    }
}
