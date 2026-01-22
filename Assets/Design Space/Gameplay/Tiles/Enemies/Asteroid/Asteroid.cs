using System;
using System.Linq;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float gravity;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private TilePlacerReference tilePlacerReference;
    [SerializeField] private float bottomOffset;
    [SerializeField] private float bottomDuration;
    [SerializeField] private LayerMask groundMask;
    
    private Vector2 startingPosition;
    private float bottomPosition;
    private float bottomTimer;
    
    private void Start()
    {
        startingPosition = rigidbody.position;

        var bottomHit = Physics2D.Raycast(rigidbody.position, Vector2.down, Mathf.Infinity, groundMask);
        bottomPosition = (bottomHit
            ? bottomHit.point.y
            : tilePlacerReference.value.Rect.min.y) - bottomOffset;
    }

    private void Update()
    {
        if (rigidbody.position.y <= bottomPosition)
        {
            bottomTimer += Time.deltaTime;
            
            rigidbody.position = new Vector2(startingPosition.x, bottomPosition);
            rigidbody.linearVelocity = Vector2.zero;

            if (bottomTimer > bottomDuration)
            {
                rigidbody.linearVelocity = Vector2.up * Mathf.Sqrt(2f * gravity * (startingPosition.y - bottomPosition));
            }
        }
        else
        {
            rigidbody.linearVelocity += Vector2.down * (gravity * Time.deltaTime);
            bottomTimer = 0f;
        }
    }
}
