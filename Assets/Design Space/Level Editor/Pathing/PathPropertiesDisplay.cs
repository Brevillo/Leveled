using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PathPropertiesDisplay : MonoBehaviour
{
    [SerializeField] private EnumSpriteMapping<PathInstance.LoopingType> loopingType;
    [SerializeField] private EnumSpriteMapping<PathInstance.ActivationType> activationType;
    [SerializeField] private ToolbarBlackboard toolbarBlackboard;
    [SerializeField] private TileEditorState tileEditorState;

    [Serializable]
    private class EnumSpriteMapping<T> where T : Enum
    {
        [SerializeField] private Image enumDisplay;
        [SerializeField] private UITooltip tooltip;
        [SerializeField] private List<Mapping> mappings;
        
        [Serializable]
        private struct Mapping
        {
            public T value;
            public Sprite sprite;
            public string tooltip;
        }

        public void SetValue(T value)
        {
            var mapping = mappings.FirstOrDefault(item => item.value.Equals(value));
            
            enumDisplay.sprite = mapping.sprite;
            tooltip.Contents = mapping.tooltip;
        }
    }
    
    private void Update()
    {
        var metadata = tileEditorState.LevelInstance.GetLayerMetadata(toolbarBlackboard.editingLayer);
        
        if (metadata == null || !metadata.TryGetValue(out PathInstance pathInstance))
        {
            return;
        }

        loopingType.SetValue(pathInstance.loopingType);
        activationType.SetValue(pathInstance.activationType);
    }
}
