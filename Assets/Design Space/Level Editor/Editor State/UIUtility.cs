using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIUtility
{
    public static bool PointerOverUI
    {
        get
        {
            Vector2 mousePosition = Input.mousePosition;

            if (mousePosition.x < 0
                || mousePosition.x > Screen.width
                || mousePosition.y < 0
                || mousePosition.y > Screen.height)
            {
                return true;
            }
            
            int uiLayer = LayerMask.NameToLayer("UI");
            
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = mousePosition,
            };
            var results = new List<RaycastResult>();
            
            EventSystem.current.RaycastAll(eventData, results);

            return results.Any(result => result.gameObject.layer == uiLayer);
        }
    }
}