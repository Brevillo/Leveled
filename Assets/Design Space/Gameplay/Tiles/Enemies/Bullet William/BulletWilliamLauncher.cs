using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BulletWilliamLauncher : MonoBehaviour
{
    [SerializeField] private GameObject bulletWilliamPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private TargetingStrategy targetingStrategy;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private List<Vector2> shootDirections;
    [SerializeField] private float firstShootDelay;
    [SerializeField] private float shootFrequency;
    [SerializeField] private SoundEffect shootSound;
    [SerializeField] private GameObjectRegistry bulletWilliamRegistry;

    private const int MaxBulletWilliamses = 30;
    
    private float shootTimer;

    private void Start()
    {
        shootTimer = firstShootDelay;
    }

    private void Update()
    {
        if (targetingStrategy.activeTarget == null)
        {
            shootTimer = firstShootDelay;
            return;
        }

        shootTimer -= Time.deltaTime;

        if (shootTimer < 0)
        {
            shootTimer = shootFrequency;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (bulletWilliamRegistry.Count >= MaxBulletWilliamses) return;
        
        shootSound.Play();
        
        Vector2 toTarget = targetingStrategy.activeTarget.transform.position - transform.position;

        Vector2 shootDirection = shootDirections
            .OrderByDescending(direction => Vector2.Dot(direction, toTarget))
            .First();

        var bullet = Instantiate(bulletWilliamPrefab, bulletSpawnPoint.position, Quaternion.identity, transform);

        bullet.GetComponent<Rigidbody2D>().linearVelocity = shootDirection * bulletSpeed;
        bullet.GetComponentInChildren<SpriteRenderer>().flipX = shootDirection.x < 0;
    }
}
