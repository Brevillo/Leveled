using System;
using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Layer Filter")]
public class LayerFilter : GameObjectFilter
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private MaskType maskType;

    private enum MaskType
    {
        MustBeInMask = 0,
        MustBeOutOfMask = 1,
    }
    
    public override bool Filter(GameObject gameObject)
    {
        bool inLayerMask = layerMask.MaskContainsLayer(gameObject.layer);

        return maskType switch
        {
            MaskType.MustBeInMask => inLayerMask,
            MaskType.MustBeOutOfMask => !inLayerMask,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}