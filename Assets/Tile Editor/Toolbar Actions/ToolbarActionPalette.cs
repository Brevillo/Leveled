using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Toolbar Actions/Toolbar Actions Palette", order = 0)]
public class ToolbarActionPalette : ScriptableObject
{
    [SerializeField] private List<ToolbarAction> actions;

    public List<ToolbarAction> Actions => actions;
}
