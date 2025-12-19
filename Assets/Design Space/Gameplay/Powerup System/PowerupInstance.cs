using UnityEngine;

public abstract class PowerupInstance : MonoBehaviour
{
    public void SetPowerup(Powerup powerup)
    {
        this.powerup = powerup;
    }
    
    public void Activate(PowerupManager manager)
    {
        this.manager = manager;
        
        addTime = Time.time;

        OnActivated();
    }

    public Powerup Powerup => powerup;

    protected float LifeTimer => Time.time - addTime;
    protected PowerupManager Manager => manager;

    private PowerupManager manager;
    private float addTime;
    private Powerup powerup;

    private void OnDisable()
    {
        OnDeactivated();
    }

    protected virtual void OnActivated() { }
    protected virtual void OnDeactivated() { }
}