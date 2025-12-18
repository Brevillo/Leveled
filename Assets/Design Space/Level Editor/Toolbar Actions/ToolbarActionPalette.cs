using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ToolbarActionsFolder + "Toolbar Actions Palette")]
public class ToolbarActionPalette : ScriptableObject
{
    [SerializeField] private List<ToolbarAction> actions;

    public List<ToolbarAction> Actions => actions;
}
