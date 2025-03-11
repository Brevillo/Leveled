using UnityEngine;

[CreateAssetMenu(menuName = CreateMenuPath + "Eraser", order = CreateMenuOrder)]
public class EraserToolAction : BrushToolAction
{
    protected override GameTile DrawingTile => null;
}
