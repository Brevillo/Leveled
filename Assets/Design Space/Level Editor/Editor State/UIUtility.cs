using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIUtility
{
    private static int uiLayer = LayerMask.NameToLayer("UI");

    public static List<RaycastResult> GetPointerRaycastResults()
    {
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition,
        };
        var results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);
        
        results.Sort((a, b) =>
        {
            // Higher priority first
            int result;

            result = b.sortingLayer.CompareTo(a.sortingLayer);
            if (result != 0) return result;

            result = b.sortingOrder.CompareTo(a.sortingOrder);
            if (result != 0) return result;

            result = b.depth.CompareTo(a.depth);
            if (result != 0) return result;

            result = a.distance.CompareTo(b.distance);
            if (result != 0) return result;

            return a.index.CompareTo(b.index);
        });

        return results;
    }

    public static bool PointerOutsideOfScreen =>
        Input.mousePosition.x < 0
        || Input.mousePosition.x > Screen.width
        || Input.mousePosition.y < 0
        || Input.mousePosition.y > Screen.height;

    public static bool PointerOverUI => 
        PointerOutsideOfScreen
        || GetPointerRaycastResults().Any(result => result.gameObject.layer == uiLayer);

    public static UILayer PointerOverUILayer
    {
        get
        {
            if (PointerOutsideOfScreen) return UILayer.None;

            var results = GetPointerRaycastResults();

            if (results.Count == 0 || results[0].gameObject.layer != uiLayer) return UILayer.None;

            var gameObject = results[0].gameObject;
            
            if (gameObject.TryGetComponent(out OnUILayer onUILayer))
            {
                return onUILayer.layer;
            }

            var parentUILayer = gameObject.GetComponentInParent<OnUILayer>();

            if (parentUILayer != null) return parentUILayer.layer;
            
            return UILayer.Default;
        }
    }
}