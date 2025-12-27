using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Complex Filter")]
public class ComplexFilter : GameObjectFilter
{
    [SerializeField] private bool baseState;
    [SerializeField] private BooleanOperation booleanOperation;
    [SerializeField] private List<GameObjectFilter> filters;

    private enum BooleanOperation
    {
        AndAll = 0,
        OrAll = 1,
    }

    public override bool Filter(GameObject gameObject)
    {
        bool filtered = baseState;

        foreach (var filter in filters)
        {
            switch (booleanOperation)
            {
                case BooleanOperation.AndAll:
                    filtered &= filter.Filter(gameObject);
                    break;
                
                case BooleanOperation.OrAll:
                    filtered |= filter.Filter(gameObject);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return filtered;
    }
}