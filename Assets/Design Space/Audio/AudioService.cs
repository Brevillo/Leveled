using System;
using System.Collections.Generic;
using System.Linq;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using UnityEngine;
using UnityEngine.Audio;

public enum SFXCategory
{
    Gameplay = 0,
    Editor = 1,
}

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Audio Service")]
public class AudioService : GameService
{
    [SerializeField] private GameObjectPool audioSourcePool;
    [SerializeField] private List<SFXCategoryMixerGroupMapping> mixerGroupMappings;

    [Serializable]
    private class SFXCategoryMixerGroupMapping
    {
        public SFXCategory sfxCategory;
        public AudioMixerGroup mixerGroup;
    }
    
    private static AudioService instance;

    public new static AudioService Instance => instance;

    protected override void Initialize()
    {
        instance = this;
    }

    public AudioMixerGroup GetMixerGroupMapping(SFXCategory sfxCategory) => mixerGroupMappings
        .FirstOrDefault(mapping => mapping.sfxCategory == sfxCategory)?.mixerGroup;
    
    public Poolable GetAudioSource() => audioSourcePool.Retrieve();
}