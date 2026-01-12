using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = CreateMenuPath + "Mover")]
public class MoverToolAction : ToolbarAction
{
    [SerializeField] private InputActionReference mouseDelta;
    [SerializeField] private float deltaScaler;
    
    private Vector2 previousMousePosition;

    protected override void OnDown()
    {
        previousMousePosition = Input.mousePosition;
        mouseDelta.action.performed += OnMouseDelta;

        blackboard.hoverSelectionActive = false;
    }

    protected override void OnPressed()
    {
        Vector2 mousePosition = Input.mousePosition;

        previousMousePosition = mousePosition;
    }
    
    protected override void OnReleased()
    {
        mouseDelta.action.performed -= OnMouseDelta;

        blackboard.hoverSelectionActive = true;
    }

    private void OnMouseDelta(InputAction.CallbackContext context)
    {
        Vector2 delta =
            // mousePosition - previousMousePosition;
            // Input.mousePositionDelta * deltaScaler;
            context.ReadValue<Vector2>() * deltaScaler;
        
        SpaceUtility.Camera.transform.position -= SpaceUtility.Camera.ScreenToWorldPoint(delta) -
                                                  SpaceUtility.Camera.ScreenToWorldPoint(Vector3.zero);
    }
}
