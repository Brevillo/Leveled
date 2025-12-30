using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public interface IEditorSettingHandler<T>
{
    T Get(string key, T defaultValue);
    void Set(string key, T value);

    VisualElement GetField(string label, T value);
}

public sealed class IntEditorSettingHandler : IEditorSettingHandler<int>
{
    public int Get(string key, int defaultValue) => EditorPrefs.GetInt(key, defaultValue);
    public void Set(string key, int value) => EditorPrefs.SetInt(key, value);

    public VisualElement GetField(string label, int value) => new IntegerField(label) { value = value };
}

public sealed class FloatEditorSettingHandler : IEditorSettingHandler<float>
{
    public float Get(string key, float defaultValue) => EditorPrefs.GetFloat(key, defaultValue);
    public void Set(string key, float value) => EditorPrefs.SetFloat(key, value);

    public VisualElement GetField(string label, float value) => new FloatField(label) { value = value };
}

public sealed class StringEditorSettingHandler : IEditorSettingHandler<string>
{
    public string Get(string key, string defaultValue) => EditorPrefs.GetString(key, defaultValue);
    public void Set(string key, string value) => EditorPrefs.SetString(key, value);

    public VisualElement GetField(string label, string value) => new TextField(label) { value = value };
}

public sealed class BoolEditorSettingHandler : IEditorSettingHandler<bool>
{
    public bool Get(string key, bool defaultValue) => EditorPrefs.GetBool(key, defaultValue);
    public void Set(string key, bool value) => EditorPrefs.SetBool(key, value);

    public VisualElement GetField(string label, bool value) => new Toggle(label) { value = value };
}

public sealed class ColorEditorSettingHandler : IEditorSettingHandler<Color>
{
    public Color Get(string key, Color defaultValue) => ColorUtility.TryParseHtmlString(
        EditorPrefs.GetString(key, ColorUtility.ToHtmlStringRGBA(defaultValue)), out var parsedColor) 
            ? parsedColor
            : defaultValue;
    
    public void Set(string key, Color value) => EditorPrefs.SetString(key, ColorUtility.ToHtmlStringRGBA(value));

    public VisualElement GetField(string label, Color value) => new ColorField(label) { value = value };
}

public sealed class ObjectEditorSettingHandler<T> : IEditorSettingHandler<T> where T : Object
{
    public T Get(string key, T defaultValue)
    {
        var data = EditorPrefs.GetString(key).Split("::");

        return 
            data.Length < 2 ? defaultValue 
            : AssetDatabase.LoadAllAssetsAtPath(data[0]).FirstOrDefault(asset => asset.name == data[1]) is T t ? t 
            : defaultValue;
    }

    public void Set(string key, T value) =>
        EditorPrefs.SetString(key, value != null ? $"{AssetDatabase.GetAssetPath(value)}::{value.name}" : string.Empty);

    public VisualElement GetField(string label, T value) =>
        new ObjectField(label) { value = value, objectType = typeof(T), allowSceneObjects = false };
}

public sealed class EnumEditorSettingHandler<T> : IEditorSettingHandler<T> where T : struct, Enum
{
    public T Get(string key, T defaultValue)
    {
        int stored = EditorPrefs.GetInt(key, Convert.ToInt32(defaultValue));
        
        return Enum.IsDefined(typeof(T), stored)
            ? (T)Enum.ToObject(typeof(T), stored)
            : defaultValue;
    }

    public void Set(string key, T value) => EditorPrefs.SetInt(key, Convert.ToInt32(value));

    public VisualElement GetField(string label, T value) => new EnumField(label, value);
}

public static class EditorSettingHandler<T>
{
    public static readonly IEditorSettingHandler<T> Instance = Create();

    private static IEditorSettingHandler<T> Create()
    {
        var t = typeof(T);

        if (t.IsEnum)
        {
            var handlerType = typeof(EnumEditorSettingHandler<>).MakeGenericType(t);
            return (IEditorSettingHandler<T>)Activator.CreateInstance(handlerType);
        }

        if (t == typeof(int)) return (IEditorSettingHandler<T>)(object)new IntEditorSettingHandler();
        if (t == typeof(float)) return (IEditorSettingHandler<T>)(object)new FloatEditorSettingHandler();
        if (t == typeof(string)) return (IEditorSettingHandler<T>)(object)new StringEditorSettingHandler();
        if (t == typeof(bool)) return (IEditorSettingHandler<T>)(object)new BoolEditorSettingHandler();
        if (t == typeof(Color)) return (IEditorSettingHandler<T>)(object)new ColorEditorSettingHandler();
        
        if (typeof(Object).IsAssignableFrom(t))
        {
            var handlerType = typeof(ObjectEditorSettingHandler<>).MakeGenericType(t);
            return (IEditorSettingHandler<T>)Activator.CreateInstance(handlerType);
        }

        throw new NotSupportedException($"Type {t} is not supported.");
    }
}

public interface IEditorSetting
{
    public VisualElement GetField(Action valueChangeAction = null);
}

public class EditorSetting<T> : IEditorSetting
{
    private readonly string editorPrefKey;
    private readonly T defaultValue;
    private readonly string fieldLabel;
    private readonly Func<T, T> valueProcessing;
    
    public EditorSetting(string editorPrefKey, T defaultValue = default, string fieldLabel = "", Func<T, T> valueProcessing = null)
    {
        this.defaultValue = defaultValue;
        this.fieldLabel = fieldLabel;
        this.valueProcessing = valueProcessing;
        this.editorPrefKey = editorPrefKey;
    }

    public T Value
    {
        get => EditorSettingHandler<T>.Instance.Get(editorPrefKey, defaultValue);
        set
        {
            var processedValue = valueProcessing != null 
                ? valueProcessing.Invoke(value) 
                : value;
            
            EditorSettingHandler<T>.Instance.Set(editorPrefKey, processedValue);
        }
    }

    public VisualElement GetField(Action valueChangeAction = null)
    {
        var field = EditorSettingHandler<T>.Instance.GetField(fieldLabel, Value);

        switch (field)
        {
            case INotifyValueChanged<T> notify:
                notify.RegisterValueChangedCallback(evt =>
                {
                    Value = evt.newValue;
                    valueChangeAction?.Invoke();
                });
                break;
          
            case INotifyValueChanged<Object> objectNotify:
                objectNotify.RegisterValueChangedCallback(evt =>
                {
                    Value = (T)(object)evt.newValue;
                    valueChangeAction?.Invoke();
                });
                break;
            
            default:
            {
                if (typeof(T).IsEnum && field is EnumField enumField)
                {
                    enumField.RegisterValueChangedCallback(evt =>
                    {
                        Value = (T)(object)evt.newValue;
                        valueChangeAction?.Invoke();
                    });
                }

                break;
            }
        }
        
        return field;
    }
}

public class ListEditorSetting<T> : IEditorSetting
{
    private readonly List<EditorSetting<T>> settings = new();

    public VisualElement GetField(Action valueChangeAction = null)
    {
        var field = new ListView(settings);

        return field;
    }
}