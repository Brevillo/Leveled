using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Cursor Template Service")]
public class CursorTemplateService : GameService
{
    [SerializeField] private CursorTemplate defaultTemplate;

    protected override void Initialize()
    {
        cursorTemplates.Clear();
        Add(defaultTemplate);
    }

    protected override void InstanceDestroyed()
    {
        Remove(defaultTemplate);
    }
    
    private static readonly HashSet<CursorTemplate> cursorTemplates = new();
    
    public static void Add(CursorTemplate template)
    {
        if (template != null)
        {
            cursorTemplates.Add(template);
        }

        RecalculateCursor();
    }

    public static void Remove(CursorTemplate template)
    {
        if (template != null)
        {
            cursorTemplates.Remove(template);
        }

        RecalculateCursor();
    }
    
    private static void RecalculateCursor()
    {
        int priority = int.MinValue;
        CursorTemplate highestTemplate = null;
        
        foreach (var template in cursorTemplates)
        {
            if (template.Priority > priority)
            {
                priority = template.Priority;
                highestTemplate = template;
            }
        }

        if (highestTemplate != null)
        {
            Cursor.SetCursor(highestTemplate.Texture, highestTemplate.Hotspot, CursorMode.Auto);
        }
    }
}