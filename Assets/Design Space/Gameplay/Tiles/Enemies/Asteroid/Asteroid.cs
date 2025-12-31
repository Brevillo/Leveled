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
    
    private Vector2 startingPosition;
    private float bottomTimer;
    
    private void Start()
    {
        startingPosition = rigidbody.position;
    }

    private void Update()
    {
        float bottom = tilePlacerReference.value.Bounds.min.y - bottomOffset;
        
        if (rigidbody.position.y <= bottom)
        {
            bottomTimer += Time.deltaTime;
            
            rigidbody.position = new Vector2(startingPosition.x, bottom);
            rigidbody.linearVelocity = Vector2.zero;

            if (bottomTimer > bottomDuration)
            {
                rigidbody.linearVelocity = Vector2.up * Mathf.Sqrt(2f * gravity * (startingPosition.y - bottom));
            }
        }
        else
        {
            rigidbody.linearVelocity += Vector2.down * (gravity * Time.deltaTime);
            bottomTimer = 0f;
        }
    }
}
