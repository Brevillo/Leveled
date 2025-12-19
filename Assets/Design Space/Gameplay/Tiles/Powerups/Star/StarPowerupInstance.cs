using UnityEngine;

public class StarPowerupInstance : PowerupInstance
{
    [SerializeField] private float duration;
    [SerializeField] private Material spriteMaterial;

    protected override void OnActivated()
    {
        Manager.SpriteMaterial.Add(this, spriteMaterial);
    }

    protected override void OnDeactivated()
    {
        Manager.SpriteMaterial.Remove(this);
    }

    private void Update()
    {
        if (LifeTimer > duration)
        {
            Destroy(gameObject);
        }
    }
}