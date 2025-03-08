using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Toolbar Actions/Toolbar Actions Palette", order = 0)]
public class ToolbarActionsPalette : ScriptableObject
{
    [SerializeField] private List<ToolbarAction> toolbarActions;

    public List<ToolbarAction> ToolbarActions => toolbarActions;
}
