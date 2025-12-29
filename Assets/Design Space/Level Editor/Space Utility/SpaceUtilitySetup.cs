using System;
using UnityEngine;

public class SpaceUtilitySetup : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Grid grid;
    [SerializeField] private SpaceUtility spaceUtility;
    
    private void Awake()
    {
        spaceUtility.Setup(mainCamera, grid);
    }
}
