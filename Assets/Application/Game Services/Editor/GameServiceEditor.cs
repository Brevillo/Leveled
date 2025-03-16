using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameService), true)]
public class ServiceEditor : Editor
{
    private GameServiceManager gameServiceManager;
    
    private GameService GameService => target as GameService;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MyEditorUtilities.DefaultManagerGUI(ref gameServiceManager,
            manager => manager.ContainsService(GameService),
            manager => manager.AddService(GameService));
    }
}
