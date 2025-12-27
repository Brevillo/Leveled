using UnityEngine;

public static class LayerMaskUtilities
{
    public static bool MaskContainsLayer(this LayerMask layerMask, int layer) => (layerMask & (1 << layer)) != 0;
}