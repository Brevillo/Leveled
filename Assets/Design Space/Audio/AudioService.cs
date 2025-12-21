using System;
using System.Collections.Generic;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using OliverBeebe.UnityUtilities.Runtime.Settings;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = ProjectConstants.ServicesFolder + "Audio Service")]
public class AudioService : GameService
{
    [SerializeField] private GameObjectPool audioSourcePool;
    [SerializeField] private List<VolumeSetting> volumeSettings;
    [SerializeField] private AudioMixer audioMixer;

    [Serializable]
    private class VolumeSetting
    {
        public string volumeParameter;
        public FloatSetting setting;

        private AudioService context;
        
        public void Initialize(AudioService context)
        {
            this.context = context;
            setting.ValueChanged += SetVolume;
            
            Update();
        }

        public void Cleanup()
        {
            setting.ValueChanged -= SetVolume;
        }

        public void Update()
        {
            SetVolume(setting.Value);
        }
        
        private void SetVolume(float value)
        {
            context.audioMixer.SetFloat(volumeParameter, PercentToDb(value));
        }
    }
    
    private static AudioService instance;

    public new static AudioService Instance => instance;

    protected override void Initialize()
    {
        instance = this;

        foreach (var volumeSetting in volumeSettings)
        {
            volumeSetting.Initialize(this);
        }
    }

    protected override void Start()
    {
        foreach (var volumeSetting in volumeSettings)
        {
            volumeSetting.Update();
        }
    }

    protected override void InstanceDestroyed()
    {
        foreach (var volumeSetting in volumeSettings)
        {
            volumeSetting.Cleanup();
        }
    }

    public Poolable GetAudioSource() => audioSourcePool.Retrieve();
    
    private static float PercentToDb(float percent) => percent == 0 ? -80f : Mathf.Log(percent) * 10f;
}