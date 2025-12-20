using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    [SerializeField] private Transform powerupsParent;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Damageable damageable;
    [SerializeField] private PowerupModifierPrioritizer<Material> spriteMaterial;
    [SerializeField] private PowerupModifierPrioritizer<Sprite> sprite;
    [SerializeField] private PowerupModifierPrioritizer<List<DamageType>> acceptedDamageTypes;

    public PowerupModifierPrioritizer<Material> SpriteMaterial => spriteMaterial;
    public PowerupModifierPrioritizer<Sprite> Sprite => sprite;
    public PowerupModifierPrioritizer<List<DamageType>> AcceptedDamageTypes => acceptedDamageTypes;

    private void Awake()
    {
        spriteMaterial.Initialize();
        sprite.Initialize();
        acceptedDamageTypes.Initialize();
    }

    private void Update()
    {
        spriteRenderer.material = spriteMaterial.CurrentValue;
        spriteRenderer.sprite = sprite.CurrentValue;
        damageable.acceptedDamageTypes = acceptedDamageTypes.CurrentValue;
    }

    public void AddPowerup(Powerup powerup)
    {
        var newPowerup = powerup.GetInstance(powerupsParent);
        
        newPowerup.Activate(this);
    }
}

public delegate TValue PrioritizationFunc<TValue>(TValue defaultValue, Dictionary<PowerupInstance, TValue> powerupValues);

[Serializable]
public class PowerupModifierPrioritizer<TValue>
{
    [SerializeField] private PowerupModifierPrioritization prioritization;
    [SerializeField] private TValue defaultValue;

    private PrioritizationFunc<TValue> prioritizationFunc;
    
    private readonly Dictionary<PowerupInstance, TValue> powerupValues = new();
    private TValue currentValue;

    public TValue CurrentValue => currentValue;

    public void Initialize(PrioritizationFunc<TValue> prioritizationFunc = null)
    {
        this.prioritizationFunc = prioritizationFunc ?? (DefaultPrioritizationFunction);
        
        ResolveCurrentValue();
    }

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
        currentValue = prioritizationFunc.Invoke(defaultValue, powerupValues);
    }

    private TValue DefaultPrioritizationFunction(
        TValue defaultValue, 
        Dictionary<PowerupInstance, TValue> powerupValues)
    {
        var currentValue = defaultValue;
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

        return currentValue;
    }
}
