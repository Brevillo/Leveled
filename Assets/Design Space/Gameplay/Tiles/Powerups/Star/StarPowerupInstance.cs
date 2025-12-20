using System.Collections.Generic;
using UnityEngine;

public class StarPowerupInstance : PowerupInstance
{
    [SerializeField] private float duration;
    [SerializeField] private Material spriteMaterial;
    [SerializeField] private List<DamageType> damageTypes;

    protected override void OnActivated()
    {
        Manager.SpriteMaterial.Add(this, spriteMaterial);
        Manager.AcceptedDamageTypes.Add(this, damageTypes);
    }

    protected override void OnDeactivated()
    {
        Manager.SpriteMaterial.Remove(this);
        Manager.AcceptedDamageTypes.Remove(this);
    }

    private void Update()
    {
        if (LifeTimer > duration)
        {
            Destroy(gameObject);
        }
    }
}