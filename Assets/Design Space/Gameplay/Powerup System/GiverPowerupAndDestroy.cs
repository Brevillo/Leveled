using UnityEngine;

public class GiverPowerupAndDestroy : MonoBehaviour
{
    [SerializeField] private Powerup powerup;
    [SerializeField] private GameObject destroy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryAddPowerup(other);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        TryAddPowerup(other.collider);
    }

    private void TryAddPowerup(Collider2D other)
    {
        if (other.TryGetComponent(out PowerupManager powerupManager))
        {
            powerupManager.AddPowerup(powerup);
            
            Destroy(destroy);
        }
    }
}