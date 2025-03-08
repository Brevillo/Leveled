using System;
using System.Reflection;
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
}