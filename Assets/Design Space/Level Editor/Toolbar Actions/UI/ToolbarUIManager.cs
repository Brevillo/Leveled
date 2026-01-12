using UnityEngine;

public class ToolbarUIManager : MonoBehaviour
{
    [SerializeField] private ToolbarActionPalette toolbarActionPalette;
    [SerializeField] private ToolbarUIItem toolbarUIItemPrefab;
    [SerializeField] private Transform itemsParent;

    private void Start()
    {
        foreach (var action in toolbarActionPalette.Actions)
        {
            Instantiate(toolbarUIItemPrefab, itemsParent).Init(action, action.SetToolEditorAction);
        }
    }
}
