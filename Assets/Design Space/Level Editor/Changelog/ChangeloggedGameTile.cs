using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "GameTile")]
public class ChangeloggedGameTile : ChangeloggedValue<GameTile>
{
    public void SwapValues(ChangeloggedGameTile other)
    {
        (Value, other.Value) = (other.Value, Value);
    }
}