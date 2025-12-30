// Code originally from the VinTools Unity Package: https://vinarkgames.itch.io/vintools
// Heavily modified by Oliver Beebe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEngine.RuleTile.TilingRuleOutput;
using static UnityEngine.RuleTile;
using Object = UnityEngine.Object;

public class RuleTileGenerator : EditorWindow
{
    [MenuItem(ProjectConstants.ToolMenuItems + "Rule Tile Generator")]
    public static void ShowWindow()
        => GetWindow<RuleTileGenerator>("Rule Tile Generator",
            Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll"));
    
    // Tilemaps
    
    private readonly EditorSetting<Texture2D> templateTilemap = 
        new(nameof(templateTilemap), fieldLabel: "Template");
    
    private readonly EditorSetting<Texture2D> tilemap = 
        new(nameof(tilemap), fieldLabel: "Tilemap");

    // Template Preview
    
    private readonly EditorSetting<int> previewColumns = 
        new(nameof(previewColumns), 7, "Columns", value => Mathf.Max(value, 1));
    
    // Rule Tile Settings
    
    private readonly EditorSetting<Tile.ColliderType> defaultColliderType =
        new(nameof(defaultColliderType), Tile.ColliderType.Sprite, "Default Collider");
    
    private readonly EditorSetting<OutputSprite> defaultOutput =
        new(nameof(defaultOutput), OutputSprite.Single, "Default Rule Output");
    
    private readonly EditorSetting<Sprite> defaultSprite = 
        new(nameof(defaultSprite), fieldLabel: "Default Sprite");
    
    private readonly EditorSetting<GameObject> defaultGameObject = 
        new(nameof(defaultGameObject), fieldLabel: "Default GameObject");

    private readonly EditorSetting<float> minAnimationSpeed =
        new(nameof(minAnimationSpeed), 1f, "Min Speed");
    
    private readonly EditorSetting<float> maxAnimationSpeed =
        new(nameof(maxAnimationSpeed), 1f, "Min Speed");

    private readonly EditorSetting<float> perlinScale =
        new(nameof(perlinScale), 0.5f, "Perlin Scale");
    
    private readonly EditorSetting<TilingRuleOutput.Transform> ruleTransform =
        new(nameof(ruleTransform), TilingRuleOutput.Transform.Fixed, "Rule Transform");

    private readonly EditorSetting<TilingRuleOutput.Transform> randomTransform =
        new(nameof(randomTransform), TilingRuleOutput.Transform.Fixed, "Shuffle");
    
    private IEditorSetting[] RuleTileSettings => new IEditorSetting[]
    {
        defaultColliderType,
        defaultGameObject,
        defaultOutput,
        minAnimationSpeed,
        maxAnimationSpeed,
        perlinScale,
        ruleTransform,
        randomTransform,
    };
    
    // Tile Generation
    
    private readonly EditorSetting<string> tileName =
        new(nameof(tileName), "Generated Rule Tile", "Tile Name");
    
    private List<int>[] templateNeighbors;
    private int defaultSpriteIndex = -1;

    private void CreateGUI()
    {
        // Error Reporting

        var errorReports = new VisualElement();
        
        var unequalSpriteCount = new HelpBox("", HelpBoxMessageType.Error);
        errorReports.Add(unequalSpriteCount);

        var noTilemapSprites = new HelpBox("Tilemap texture has no sprites.", HelpBoxMessageType.Error);
        errorReports.Add(noTilemapSprites);

        var templateReadable = new HelpBox("Template must be marked as read/write enabled.", HelpBoxMessageType.Error);
        errorReports.Add(templateReadable);
        
        var tilemapReadable = new HelpBox("Tilemap must be marked as read/write enabled.", HelpBoxMessageType.Error);
        errorReports.Add(tilemapReadable);
        
        // Asset Preview

        var assetPreview = new Foldout
        {
            text = "Generated Tile Preview",
        };
        
        var assetInspector = new VisualElement
        {
            enabledSelf = false,
            style =
            {
                backgroundColor = new Color(0.1f, 0.1f, 0.1f),
                
                marginBottom = 5f,
                marginTop = 5f,
                marginLeft = 5f,
                marginRight = 5f,

                paddingBottom = 5f,
                paddingTop = 5f,
                paddingLeft = 5f,
                paddingRight = 5f,
            },
        };
        assetPreview.Add(assetInspector);

        // Template Preview
        
        var templatePreview = new Foldout { text = "Template Preview" };

        var previewBackground = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                alignContent = Align.Center,
                overflow = Overflow.Hidden,
            },
        };
        
        templatePreview.Add((IntegerField)previewColumns.GetField(UpdateGUI));
        templatePreview.Add(previewBackground);
        
        // Rule Tile Settings
        
        var ruleTileSettings = new Foldout { text = "Rule Tile Settings" };

        var defaultSpriteField = (ObjectField)defaultSprite.GetField(UpdateAssetPreview);
        ruleTileSettings.Add(defaultSpriteField);
        
        foreach (var value in RuleTileSettings)
        {
            ruleTileSettings.Add(value.GetField(UpdateAssetPreview));
        }
        
        // Template and Tilemap Fields

        var tilemapFields = new VisualElement();
        
        tilemapFields.Add(templateTilemap.GetField(() =>
        {
            LoadTemplate();
            UpdateGUI();
        }));
        tilemapFields.Add(tilemap.GetField(() =>
        {
            UpdateGUI();

            var tileSprites = GetSprites(tilemap.Value);
            var templateSprites = GetSprites(templateTilemap.Value);
            
            if (defaultSpriteIndex != -1 && tileSprites.Length % templateSprites.Length == 0)
            {
                defaultSpriteField.value = tileSprites[defaultSpriteIndex];
            }
        }));
        
        // Tile Generation

        var tileGeneration = new VisualElement();
        
        tileGeneration.Add(tileName.GetField());
        tileGeneration.Add(new Button(() => SaveAsset(GenerateRuleTile(), tileName.Value)) { text = "Generate Tile" });
        
        // Root Construction

        var root = new ScrollView(ScrollViewMode.Vertical);
        
        foreach (var section in new[]
                 {
                     tilemapFields,
                     errorReports,
                     templatePreview,
                     ruleTileSettings,
                     tileGeneration,
                     assetPreview,
                 })
        {
            root.Add(section);
        }
        
        rootVisualElement.Add(root);

        LoadTemplate();
        UpdateGUI();

        void UpdateGUI()
        {
            // Tilemap Previews
            
            previewBackground.Clear();
            
            var tileSprites = GetSprites(tilemap.Value);
            var templateSprites = GetSprites(templateTilemap.Value);

            if (templateTilemap.Value != null && templateTilemap.Value.isReadable)
            {
                previewBackground.Add(CreateTilemapPreview(templateSprites, previewColumns.Value));
            }

            if (tilemap.Value != null && tilemap.Value.isReadable)
            {
                int tileCount = templateSprites.Length;
                
                for (int i = 0; i < tileSprites.Length / tileCount; i++)
                {
                    var tiles = tileSprites[(tileCount * i)..((i + 1) * tileCount)];

                    previewBackground.Add(CreateTilemapPreview(tiles, previewColumns.Value));
                }
            }

            // Error Reporting

            unequalSpriteCount.SetDisplayed(tileSprites.Length % templateSprites.Length != 0);
            unequalSpriteCount.text =
                "Tilemap must have a multiple of the number of template tiles.\n" + 
                $"Template has {templateSprites.Length} sprites.\n" +
                $"Tilemap has {tileSprites.Length} sprites.";
            
            noTilemapSprites.SetDisplayed(tileSprites.Length == 0);
            
            templateReadable.SetDisplayed(templateTilemap.Value == null || !templateTilemap.Value.isReadable);
            tilemapReadable.SetDisplayed(tilemap.Value == null || !tilemap.Value.isReadable);
            
            UpdateAssetPreview();
        }

        void UpdateAssetPreview()
        {
            assetInspector.Clear();
            
            var tileSprites = GetSprites(tilemap.Value);
            var templateSprites = GetSprites(templateTilemap.Value);

            if (tilemap.Value == null || templateTilemap.Value == null 
                || !tilemap.Value.isReadable || !templateTilemap.Value.isReadable
                || tileSprites.Length % templateSprites.Length != 0)
            {
                return;
            }

            var asset = GenerateRuleTile();
            var editor = Editor.CreateEditor(asset);
            
            var container = new IMGUIContainer
            {
                onGUIHandler = () =>
                {
                    if (editor != null)
                    {
                        editor.OnInspectorGUI();
                    }
                },
            };

            assetInspector.Add(container);
        }
    }

    private RuleTile GenerateRuleTile()
    {
        var tile = CreateInstance<ExtraNeighbors>();

        // Set default tile
        tile.m_DefaultSprite = defaultSprite.Value;
        tile.m_DefaultColliderType = defaultColliderType.Value;
        tile.m_DefaultGameObject = defaultGameObject.Value;

        int templateSpriteCount = GetSprites(templateTilemap.Value).Length;
        var tileSprites = GetSprites(tilemap.Value);

        // Set tiling rules
        tile.m_TilingRules = Enumerable.Range(0, templateSpriteCount)
            .Select(i => new TilingRule 
            { 
                m_Sprites = Enumerable.Range(0, tileSprites.Length / templateSpriteCount)
                    .Select(offset => tileSprites[i + offset * templateSpriteCount])
                    .ToArray(),
                
                m_GameObject = tile.m_DefaultGameObject,
                m_MinAnimationSpeed = minAnimationSpeed.Value,
                m_MaxAnimationSpeed = maxAnimationSpeed.Value,
                m_PerlinScale = perlinScale.Value,
                m_Output = defaultOutput.Value,
                m_Neighbors = templateNeighbors[i],
                m_RuleTransform = ruleTransform.Value,
                m_ColliderType = tile.m_DefaultColliderType,
                m_RandomTransform = randomTransform.Value,
            })
            .ToList();
        
        return tile;
    }
    
    private void LoadTemplate()
    {
        if (templateTilemap.Value == null || !templateTilemap.Value.isReadable) return;

        var sprites = GetSprites(templateTilemap.Value);
        
        templateNeighbors = new List<int>[sprites.Length];
        defaultSpriteIndex = -1;
        
        for (int i = 0; i < sprites.Length; i++)
        {
            var sprite = sprites[i];
            
            Vector2Int offset = Vector2Int.FloorToInt(sprite.rect.position);
            Vector2Int size = Vector2Int.FloorToInt(sprite.rect.size);
            
            // Gather sprite colors
            var neighborColors = new TilingRule().m_NeighborPositions
                .Select(neighbor => sprite.texture.GetPixel(
                    offset.x + neighbor.x switch
                    {
                        0 => size.x / 2,
                        1 => size.x - 1,
                        _ => 0,
                    },
                    offset.y + neighbor.y switch
                    {
                        0 => size.y / 2,
                        1 => size.y - 1,
                        _ => 0,
                    }))
                .ToArray();

            // Set default sprite if no green
            if (!neighborColors.Contains(Color.green))
            {
                defaultSpriteIndex = i;
            }

            // Set rules based on the color of the pixels
            templateNeighbors[i] = neighborColors
                .Select(color => (color.r, color.g, color.b) switch
                {
                    (1, 0, 0) => Neighbor.NotThis,
                    (0, 1, 0) => Neighbor.This,
                    _ => 0,
                })
                .ToList();
        }
    }

    private static VisualElement CreateTilemapPreview(Sprite[] sprites, int columns)
    {
        var root = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                alignItems = Align.Center,
                flexGrow = 1f,
            },
        };
        
        int rows = sprites.Length / columns + (sprites.Length % columns > 0 ? 1 : 0);

        float tileMargin = 2f;
        float tileSize = 30f;
            
        int i = 0;
            
        for (int y = 0; y < rows; y++)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    height = tileSize,

                    marginBottom = tileMargin,
                    marginTop = tileMargin,
                },
            };
            root.Add(row);
                
            for (int x = 0; x < columns; x++)
            {
                row.Add(new Image
                {
                    image = i < sprites.Length
                        ? CreateSpriteTexture(sprites[i])
                        : null,

                    style =
                    {
                        flexGrow = 1f,
                        height = tileSize,
                        width = tileSize,
                                                    
                        marginLeft = tileMargin,
                        marginRight = tileMargin,
                    },
                        
                });
                    
                i++;
            }
        }

        return root;
    }
    
    private static Texture2D CreateSpriteTexture(Sprite sprite)
    {
        // Get slice data
        var slice = new RectInt(
            Vector2Int.FloorToInt(sprite.rect.position), 
            Vector2Int.FloorToInt(sprite.rect.size));
        
        var colors = sprite.texture.GetPixels(slice.x, slice.y, slice.width, slice.height);

        // Create texture
        Texture2D texture = new Texture2D(slice.width, slice.height, TextureFormat.ARGB32, false);
        texture.SetPixels(0, 0, slice.width, slice.height, colors);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }
    
    private static void SaveAsset(Object tile, string name)
    {
        string folder = TryGetActiveFolderPath(out string activeFolder) ? activeFolder : "Assets";
        AssetDatabase.CreateAsset(tile, $"{folder}/{name}.asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = tile;
    }
    
    private static bool TryGetActiveFolderPath( out string path )
    {
        var tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic );

        object[] args = { null };
        bool found = (bool)tryGetActiveFolderPath?.Invoke( null, args );
        path = (string)args[0];

        return found;
    }
    
    private static Sprite[] GetSprites(Texture2D texture2D) => texture2D == null 
        ? Array.Empty<Sprite>()
        : AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture2D))
            .Where(asset => asset != texture2D)
            .OrderBy(asset => PadNumbers(asset.name))
            .OfType<Sprite>()
            .ToArray();

    private static string PadNumbers(string input) => 
        Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
};