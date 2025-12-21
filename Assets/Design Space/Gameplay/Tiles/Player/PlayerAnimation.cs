using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private float invincibilityFlashSpeed;
    [SerializeField] private float invincibilityAlpha;
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private Targetable targetable;
    [SerializeField] private Transform visuals;

    private float invincibilityFlashTimer;
    
    private void Update()
    {
        if (characterHealth.Invincible)
        {
            invincibilityFlashTimer += Time.deltaTime;

            if (invincibilityFlashTimer > invincibilityFlashSpeed)
            {
                invincibilityFlashTimer = 0f;

                spriteRenderer.color = spriteRenderer.color == Color.white
                    ? new(1f, 1f, 1f, invincibilityAlpha)
                    : Color.white;
            }
        }
        else
        {
            invincibilityFlashTimer = 0f;
            spriteRenderer.color = Color.white;
        }

        float xInput = Mathf.Sign(moveInput.action.ReadValue<Vector2>().x);
       
        if (xInput != 0f)
        {
            targetable.facingDirection = new(xInput, 0f);
            visuals.localScale = new(xInput, 1f, 1f);
        }
    }
}
