using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ContentFolder + "Powerup")]
public class Powerup : ScriptableObject
{
    [SerializeField] private PowerupInstance powerupInstancePrefab;

    public PowerupInstance GetInstance(Transform parent)
    {
        var instance = Instantiate(powerupInstancePrefab, parent);
        
        instance.SetPowerup(this);
        
        return instance;
    }
}