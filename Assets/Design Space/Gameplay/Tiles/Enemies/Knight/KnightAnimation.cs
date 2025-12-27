using System;
using UnityEngine;

public class KnightAnimation : MonoBehaviour
{
    [SerializeField] private Sprite fullHealth;
    [SerializeField] private Sprite halfHealth;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CharacterHealth health;
    
    private void Awake()
    {
        health.HitpointsUpdated += OnHitpointsUpdated;
    }

    private void OnHitpointsUpdated()
    {
        spriteRenderer.sprite = health.HitpointsPercent == 1f
            ? fullHealth
            : halfHealth;
    }
}
