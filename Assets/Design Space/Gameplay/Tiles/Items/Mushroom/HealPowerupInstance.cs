using System.Collections.Generic;
using UnityEngine;

public class HealPowerupInstance : PowerupInstance
{
    [SerializeField] private int healAmount;
    
    protected override void OnActivated()
    {
        Manager.GetComponent<CharacterHealth>().Heal(healAmount);
        Destroy(gameObject);
    }
}
