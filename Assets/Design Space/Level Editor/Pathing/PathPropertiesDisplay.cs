using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PathPropertiesDisplay : MonoBehaviour
{
    [SerializeField] private Image loopingTypeDisplay;
    [SerializeField] private EnumSpriteMapping<PathInstance.LoopingType> loopingTypeSprites;
    [SerializeField] private Image activationTypeDisplay;
    [SerializeField] private EnumSpriteMapping<PathInstance.ActivationType> activationTypeSprites;
    [SerializeField] private ToolbarBlackboard toolbarBlackboard;
    [SerializeField] private TileEditorState tileEditorState;

    [Serializable]
    private class EnumSpriteMapping<T> where T : Enum
    {
        [SerializeField] private List<Mapping> mappings;
        
        [Serializable]
        private struct Mapping
        {
            public T value;
            public Sprite sprite;
        }

        public Sprite GetSprite(T value) => mappings.FirstOrDefault(item => item.value.Equals(value)).sprite;
    }
    
    private void Update()
    {
        if (!tileEditorState.LevelInstance.GetLayerMetadata(toolbarBlackboard.editingLayer)
                .TryGetValue(out PathInstance pathInstance))
        {
            return;
        }

        loopingTypeDisplay.sprite = loopingTypeSprites.GetSprite(pathInstance.loopingType);
        activationTypeDisplay.sprite = activationTypeSprites.GetSprite(pathInstance.activationType);
    }
}
