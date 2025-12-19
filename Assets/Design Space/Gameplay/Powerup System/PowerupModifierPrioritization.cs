using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ContentFolder + "Powerup Modifier Prioritization")]
public class PowerupModifierPrioritization : ScriptableObject
{
    [Header("Top is highest priority, bottom is lowest priority")]
    [SerializeField] private List<Powerup> prioritization;

    public int GetPriority(Powerup powerup)
    {
        int index = prioritization.IndexOf(powerup);

        return index != -1
            ? prioritization.Count - index // Gives the top list entry the highest number
            : 0;
    }
}