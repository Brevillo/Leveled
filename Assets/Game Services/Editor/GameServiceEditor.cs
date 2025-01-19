using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameService), true)]
public class ServiceEditor : Editor
{
    private GameServiceManager gameServiceManager;
    
    private const string
        NoManagersError       = "No Game Service Managers Found!",
        MultipleManagersError = "Multiple Game Service Managers Found!",
        AddToManagerButton    = "Add to Game Service Manager";

    private GameService GameService => target as GameService;

    private GameServiceManager FindManager(out string errorMessage)
    {
        errorMessage = "";
        
        // already has manager
        if (gameServiceManager != null)
        {
            return gameServiceManager;
        }

        var managerGUIDs = AssetDatabase.FindAssets($"t:{nameof(GameServiceManager)}");
        
        switch (managerGUIDs.Length)
        {
            // no managers found
            case 0:
                errorMessage = NoManagersError;
                return null;
            
            // multiple managers found
            case > 1:
                errorMessage = MultipleManagersError;
                return null;
        }

        // find manager asset
        string managerPath = AssetDatabase.GUIDToAssetPath(managerGUIDs[0]);
        return AssetDatabase.LoadAssetAtPath<GameServiceManager>(managerPath);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        var manager = FindManager(out var errorMessage);
        bool onManager = manager != null && manager.ContainsService(GameService);

        // display connected manager
        if (onManager)
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Game Service Manager", gameServiceManager, typeof(GameServiceManager), false);
            GUI.enabled = true;
        }

        // error message
        else if (errorMessage != "")
        {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error, true);
        }

        // prompt to add service to manager
        else if (GUILayout.Button(AddToManagerButton))
        {
            manager.AddService(GameService);
            EditorUtility.SetDirty(manager);

            gameServiceManager = manager;
            EditorUtility.SetDirty(GameService);

            AssetDatabase.SaveAssets();
        }
    }
}
