using UnityEngine;

public class StarPowerupInstance : PowerupInstance
{
    [SerializeField] private float duration;
    [SerializeField] private Material spriteMaterial;

    protected override void OnActivated()
    {
        Manager.SpriteMaterial.Add(this, spriteMaterial);
        Manager.Invinicible.Add(this, true);
    }

    protected override void OnDeactivated()
    {
        Manager.SpriteMaterial.Remove(this);
        Manager.Invinicible.Remove(this);
    }

    private void Update()
    {
        if (LifeTimer > duration)
        {
            Destroy(gameObject);
        }
    }
}