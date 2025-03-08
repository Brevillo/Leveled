using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ToolbarAction), true)]
public class ToolbarActionEditor : Editor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) =>
        MyEditorUtilities.GetSpritePreview(
            ((ToolbarAction)target).IconSprite,
            (assetPath, subAssets, width, height),
            base.RenderStaticPreview);
}
