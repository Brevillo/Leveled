using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(GameTile))]
public class GameTileEditor : Editor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) =>
        MyEditorUtilities.GetSpritePreview(
            ((GameTile)target).PaletteIcon,
            (assetPath, subAssets, width, height),
            base.RenderStaticPreview);
}