using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using Object = UnityEngine.Object;

[CustomEditor(typeof(GameTile))]
public class GameTileEditor : Editor
{
    private Sprite Sprite => target is GameTile tile ? tile.PaletteIcon : null;

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        if (Sprite == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);
        
        var type = GetType("UnityEditor.SpriteUtility");
        if (type == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);
        
        var method = type.GetMethod("RenderStaticPreview", new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) });
        if (method == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);
        
        var ret = method.Invoke("RenderStaticPreview", new object[] { Sprite, Color.white, width, height });
        if (ret is Texture2D texture2D) return texture2D;
        
        return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }

    private static Type GetType(string typeName)
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
