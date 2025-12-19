using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    [SerializeField] private Transform powerupsParent;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PowerupModifierPrioritizer<Material> spriteMaterial;
    [SerializeField] private PowerupModifierPrioritizer<Sprite> sprite;

    public PowerupModifierPrioritizer<Material> SpriteMaterial => spriteMaterial;
    public PowerupModifierPrioritizer<Sprite> Sprite => sprite;

    private void Update()
    {
        foreach (var prioritizer in new IPowerupModifierPrioritizer[]
                 {
                     spriteMaterial,
                     sprite,
                 })
        {
            prioritizer.ResolveCurrentValue();
        }
        
        spriteRenderer.material = spriteMaterial.CurrentValue;
        spriteRenderer.sprite = sprite.CurrentValue;
    }

    public void AddPowerup(Powerup powerup)
    {
        var newPowerup = powerup.GetInstance(powerupsParent);
        
        newPowerup.Activate(this);
    }
}

public interface IPowerupModifierPrioritizer
{
    void ResolveCurrentValue();
}

[Serializable]
public class PowerupModifierPrioritizer<TValue> : IPowerupModifierPrioritizer
{
    [SerializeField] private PowerupModifierPrioritization prioritization;
    [SerializeField] private TValue defaultValue;

    private readonly Dictionary<PowerupInstance, TValue> powerupValues = new();
    private TValue currentValue;

    public TValue CurrentValue => currentValue;

    public void Add(PowerupInstance powerup, TValue value)
    {
        powerupValues.Add(powerup, value);
        ResolveCurrentValue();
    }

    public void Remove(PowerupInstance powerup)
    {
        powerupValues.Remove(powerup);
        ResolveCurrentValue();
    }

    public void ResolveCurrentValue()
    {
        currentValue = defaultValue;
        int highestPriority = int.MinValue;
        
        foreach (var kv in powerupValues)
        {
            int priority = prioritization.GetPriority(kv.Key.Powerup);

            if (priority > highestPriority)
            {
                highestPriority = priority;
                currentValue = kv.Value;
            }
        }
    }
}