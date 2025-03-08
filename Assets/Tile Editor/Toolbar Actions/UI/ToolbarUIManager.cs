using System;
using UnityEngine;

public class ToolbarUIManager : MonoBehaviour
{
    [SerializeField] private ToolbarActionsPalette toolbarActionsPalette;
    [SerializeField] private ToolbarUIItem toolbarUIItemPrefab;
    [SerializeField] private Transform itemsParent;

    private void Awake()
    {
        foreach (var toolbarAction in toolbarActionsPalette.ToolbarActions)
        {
            Instantiate(toolbarUIItemPrefab, itemsParent).Init(toolbarAction);
        }
    }
}
