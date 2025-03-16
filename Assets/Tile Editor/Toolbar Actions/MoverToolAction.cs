using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = CreateMenuPath + "Mover", order = CreateMenuOrder)]
public class MoverToolAction : ToolbarAction
{
    private Vector2 mousePosition;
    
    protected override void OnDown()
    {
        mousePosition = Input.mousePosition;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    protected override void OnPressed()
    {
        Vector2 delta = Input.mousePositionDelta * 4f;
        SpaceUtility.Camera.transform.position -= SpaceUtility.Camera.ScreenToWorldPoint(delta) -
                                                  SpaceUtility.Camera.ScreenToWorldPoint(Vector3.zero);

        mousePosition += delta;
    }

    protected override void OnReleased()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Vector2 viewportPosition = SpaceUtility.Camera.ScreenToViewportPoint(mousePosition);
        
        Mouse.current.WarpCursorPosition(
            viewportPosition.x < 0 
            || viewportPosition.x > 1
            || viewportPosition.y < 0
            || viewportPosition.y > 1
                ? SpaceUtility.Camera.ViewportToScreenPoint(Vector2.one / 2f)
                : mousePosition);

        blackboard.snapHoverSelection = true;
    }
}
