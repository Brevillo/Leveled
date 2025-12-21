using System;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = ProjectConstants.AudioFolder + "Sound Effect")]
public class SoundEffect : ScriptableObject
{
    [SerializeField] private AudioResource audioResource;
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private bool preventOverlap;
    [SerializeField] private float maxPlayFrequency;
    
    private Poolable activeSource;
    private float lastPlayTime;
    
    public void Play()
    {
        if (preventOverlap && activeSource != null) return;

        if (Time.time < lastPlayTime) lastPlayTime = 0f;
        
        if (Time.time < lastPlayTime + maxPlayFrequency) return;
        lastPlayTime = Time.time;
        
        activeSource = AudioService.Instance.GetAudioSource();
        activeSource.Returned += OnActiveSourceReturned;
        
        var audioSource = activeSource.GetComponent<AudioSource>(); 

        audioSource.resource = audioResource;
        audioSource.outputAudioMixerGroup = mixerGroup;
        audioSource.Play();
    }

    private void OnActiveSourceReturned(Poolable poolable)
    {
        if (activeSource == null) return;
        
        activeSource.Returned -= OnActiveSourceReturned;
        activeSource = null;
    }
}