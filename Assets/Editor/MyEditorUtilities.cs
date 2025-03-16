using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class MyEditorUtilities
{
    public static Texture2D GetSpritePreview(
        Sprite sprite,
        (string assetPath, Object[] subAssets, int width, int height) parameters, System.Func<string, Object[], int, int, Texture2D> baseRenderStaticPreview)
    {
        if (sprite == null) return Base();
        
        var type = GetSystemType("UnityEditor.SpriteUtility");
        if (type == null) return Base();

        var method = type.GetMethod("RenderStaticPreview",
            new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) });
        if (method == null) return Base();

        return method.Invoke("RenderStaticPreview", 
                new object[] { sprite, Color.white, parameters.width, parameters.height }) 
            as Texture2D ?? Base();

        Texture2D Base() => baseRenderStaticPreview.Invoke(
            parameters.assetPath,
            parameters.subAssets, 
            parameters.width,
            parameters.height);
    }

    private static Type GetSystemType(string typeName)
    {
        var type = Type.GetType(typeName);
        if (type != null) return type;

        var currentAssembly = Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        
        foreach (var assemblyName in referencedAssemblies)
        {
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null) continue;
            
            type = assembly.GetType(typeName);
            if (type != null) return type;
        }
        
        return null;
    }
    
    public static bool FindManager<TManager>(ref TManager manager, out string errorMessage)
        where TManager : Object
    {
        errorMessage = "";
        
        // already has manager
        if (manager != null)
        {
            return true;
        }

        var managerGUIDs = AssetDatabase.FindAssets($"t:{typeof(TManager).Name}");
        
        switch (managerGUIDs.Length)
        {
            // no managers found
            case 0:
                errorMessage = $"No {typeof(TManager).Name} found!";
                return false;
            
            // multiple managers found
            case > 1:
                errorMessage = $"Multiple {typeof(TManager).Name}s found!";
                return false;
            
            // found manager asset
            default:
                string path = AssetDatabase.GUIDToAssetPath(managerGUIDs[0]);
                manager = AssetDatabase.LoadAssetAtPath<TManager>(path);
                return true;
        }
    }

    public static void DefaultManagerGUI<TManager>(
        ref TManager manager, 
        Func<TManager, bool> managerContains, 
        Action<TManager> addToManager) 
        where TManager : Object
    {
        EditorGUILayout.Space();

        bool onManager = FindManager(ref manager, out var errorMessage)
                         && managerContains.Invoke(manager);

        // display connected manager
        if (onManager)
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField($"{typeof(TManager).Name}", manager, typeof(TManager), false);
            GUI.enabled = true;
        }

        // error message
        else if (errorMessage != "")
        {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error, true);
        }

        // prompt to add service to manager
        else if (GUILayout.Button($"Add to {typeof(TManager).Name}"))
        {
            addToManager.Invoke(manager);
            EditorUtility.SetDirty(manager);

            AssetDatabase.SaveAssets();
        }

    }
}