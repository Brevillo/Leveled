using OliverBeebe.UnityUtilities.Runtime.Pooling;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Canvas GameObject Pool")]
public class CanvasGameObjectPool : GameObjectPool
{
    private Transform parent;

    public void SetParent(Transform parent) => this.parent = parent;

    protected override Transform SpawnHeirarchyParent() => parent;
}
