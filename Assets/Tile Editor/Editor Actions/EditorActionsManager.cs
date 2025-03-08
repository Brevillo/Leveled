using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Editor Actions/Editor Action Palette", order = 0)]
public class EditorActionsManager : GameService
{
    [SerializeField] private List<EditorAction> editorActions;
    [SerializeField] private GameStateManager gameStateManager;

    protected override void Initialize()
    {
        EnableAll();
    }

    protected override void InstanceDestroyed()
    {
        DisableAll();
    }

    public void EnableAll()
    {
        foreach (var action in editorActions)
        {
            action.Enable(gameStateManager);
        }
    }

    public void DisableAll()
    {
        foreach (var action in editorActions)
        {
            action.Disable();
        }
    }
}
