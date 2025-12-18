using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
    [SerializeField] private List<string> teams;

    public Vector2 facingDirection;

    public List<string> Teams => teams;
}