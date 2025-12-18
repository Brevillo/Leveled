using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MenuItemUtilities
{
    [MenuItem(ProjectConstants.ToolMenuItems + "Reload Scene &r")]
    private static void ReloadScene()
    {
        if (Application.isPlaying)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(SceneManager.GetActiveScene().name);
        }
    }
    
    [MenuItem(ProjectConstants.ToolMenuItems + "Save Assets &s")]
    private static void SaveAssets()
    {
        AssetDatabase.SaveAssets();
    }
}
