using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
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
    
    public static bool SelectionIsPrefabsWithComponent<T>()
        => Selection.objects.Length > 0 && Selection.objects
            .All(obj => obj is GameObject go 
                        && go.TryGetComponent(out T _)
                        && PrefabUtility.IsPartOfPrefabAsset(go));

    public static Object[] CreateScriptableObjectFromAssetSelection<TAsset, TScriptableObject>(
        Action<TAsset, TScriptableObject> setupAction,
        Func<TAsset, string> namingScheme,
        bool selectCreated = true)
        where TAsset : Object
        where TScriptableObject : ScriptableObject
    {
        List<Object> createdScriptableObjects = new();
        
        foreach (var guid in Selection.assetGUIDs)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TAsset>(AssetDatabase.GUIDToAssetPath(guid));
            
            if (asset == null) continue;
            
            var scriptableObject = ScriptableObject.CreateInstance<TScriptableObject>();
            setupAction.Invoke(asset, scriptableObject);
            
            string selectionPath = AssetDatabase.GUIDToAssetPath(guid);
            string assetPath = $"{Path.GetDirectoryName(selectionPath)}/{namingScheme.Invoke(asset)}.asset";

            if (AssetDatabase.AssetPathExists(assetPath))
            {
                Debug.LogWarning($"{typeof(TScriptableObject).Name} already exists for {asset.name}!");
                continue;
            }
            
            createdScriptableObjects.Add(scriptableObject);
            AssetDatabase.CreateAsset(scriptableObject, assetPath);
        }

        var created = createdScriptableObjects.ToArray();
        
        if (selectCreated)
        {
            Selection.objects = created;
        }

        return created;
    }

    public static void SetDisplayed(this VisualElement visualElement, bool display)
    {
        visualElement.style.display = new(display ? DisplayStyle.Flex : DisplayStyle.None);
    }
    
    public static string GetActiveFolderPath()
    {
        var tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic );

        object[] args = { null };
        bool found = (bool)tryGetActiveFolderPath?.Invoke( null, args );
        string path = (string)args[0];

        return found ? path : "Assets";
    }
    
    public static T[] GetSubAssets<T>(Object asset) where T : Object => asset == null 
        ? Array.Empty<T>()
        : AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset))
            .Where(subAsset => subAsset != asset)
            .OrderBy(subAsset => PadNumbers(subAsset.name))
            .OfType<T>()
            .ToArray();
    
    private static string PadNumbers(string input) => 
        Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
}