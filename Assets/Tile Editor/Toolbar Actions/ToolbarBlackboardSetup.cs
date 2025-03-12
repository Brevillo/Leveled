using System;
using UnityEngine;

public class ToolbarBlackboardSetup : MonoBehaviour
{
    [SerializeField] private TilePlacer tilePlacer;
    [SerializeField] private LinkingGroupSetter linkingGroupSetter;
    [SerializeField] private ToolbarBlackboard toolbarBlackboard;

    private void Awake()
    {
        toolbarBlackboard.tilePlacer = tilePlacer;
        toolbarBlackboard.linkingGroupSetter = linkingGroupSetter;
    }
}
