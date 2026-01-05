using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Bool")]
public class ChangeloggedBool : ChangeloggedValue<bool>
{
    public void ToggleValue() => Value = !Value;
}
