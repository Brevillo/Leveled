using TMPro;
using UnityEngine;

public class SelectionUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI width;
    [SerializeField] private TextMeshProUGUI height;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float moveSpeed;

    private Vector2 min, minVelocity;
    private Vector2 max, maxVelocity;
    
    public void SetSize(BoundsInt bounds, bool snap = false)
    {
        Vector2 minCanvas = GetCanvasPosition(bounds.min, ref min, ref minVelocity);
        Vector2 maxCanvas = GetCanvasPosition(bounds.max, ref max, ref maxVelocity);
        
        rectTransform.sizeDelta = maxCanvas - minCanvas;
        
        rectTransform.position = (maxCanvas + minCanvas) / 2f;

        width.enabled = height.enabled = bounds.size.x != 1 || bounds.size.y != 1;

        width.text = bounds.size.x.ToString();
        height.text = bounds.size.y.ToString();
        
        Vector2 GetCanvasPosition(Vector3Int targetCell, ref Vector2 position, ref Vector2 velocity)
        {
            Vector2 target = spaceUtility.CellToWorld((Vector2Int)targetCell) - Vector3.one / 2f;
            
            position = snap 
                ? target
                : Vector2.SmoothDamp(position, target, ref velocity, moveSpeed);
            
            return spaceUtility.WorldToCanvas(position, rectTransform);
        }
    }

}
