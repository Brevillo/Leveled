using UnityEngine;

[CreateAssetMenu(menuName = CreateMenuPath + "Eraser")]
public class EraserToolAction : BrushToolAction
{
    protected override GameTile DrawingTile => null;
}
