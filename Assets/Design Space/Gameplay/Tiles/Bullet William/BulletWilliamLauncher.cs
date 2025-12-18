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
    [SerializeField] private float shootFrequency;
    [SerializeField] private SoundEffect shootSound;

    private float shootTimer;
    
    private void Update()
    {
        if (targetingStrategy.ActiveTarget == null)
        {
            shootTimer = 0f;
            return;
        }

        shootTimer += Time.deltaTime;

        if (shootTimer > shootFrequency)
        {
            shootTimer = 0f;
            Shoot();
        }
    }

    private void Shoot()
    {
        shootSound.Play();
        
        Vector2 toTarget = targetingStrategy.ActiveTarget.transform.position - transform.position;

        Vector2 shootDirection = shootDirections
            .OrderByDescending(direction => Vector2.Dot(direction, toTarget))
            .First();

        var bullet = Instantiate(bulletWilliamPrefab, bulletSpawnPoint.position, Quaternion.identity, transform);

        bullet.GetComponent<Rigidbody2D>().linearVelocity = shootDirection * bulletSpeed;
        bullet.GetComponentInChildren<SpriteRenderer>().flipX = shootDirection.x < 0;
    }
}
