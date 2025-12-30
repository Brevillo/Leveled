using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Extra Neighbors Rule Tile" , menuName = "2D/Tiles/Extra Neighbors Rule Tile", order = 999)]
public class ExtraNeighbors : RuleTile<RuleTile.TilingRuleOutput.Neighbor> 
{
    [SerializeField] private List<TileBase> neighbors = new();

    public override bool RuleMatch(int neighbor, TileBase tile) 
    {
        if (tile is RuleOverrideTile ruleOverrideTile) tile = ruleOverrideTile.m_InstanceTile;

        return neighbor switch 
        {
            TilingRuleOutput.Neighbor.This => tile == this || neighbors.Contains(tile),
            TilingRuleOutput.Neighbor.NotThis => !(tile == this || neighbors.Contains(tile)),
            _ => base.RuleMatch(neighbor, tile),
        };
    }
}