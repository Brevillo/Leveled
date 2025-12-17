using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetingStrategy : MonoBehaviour
{
    [SerializeField] private List<string> targetTeams;
    
    public Targetable ActiveTarget => activeTarget;
    
    protected Targetable activeTarget;

    protected bool TargetableFilter(Targetable targetable) =>
        targetTeams.Exists(team => targetable.Teams.Contains(team));
}