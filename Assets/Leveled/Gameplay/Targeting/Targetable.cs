using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
    [SerializeField] private List<string> teams;

    public List<string> Teams => teams;
}