using System;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = ProjectConstants.AudioFolder + "Sound Effect")]
public class SoundEffect : ScriptableObject
{
    [SerializeField] private AudioResource audioResource;
    [SerializeField] private SFXCategory sfxCategory;
    [SerializeField] private bool allowOverlap;
    
    private Poolable activeSource;
    
    public void Play()
    {
        if (!allowOverlap && activeSource != null) return;
        
        activeSource = AudioService.Instance.GetAudioSource();
        activeSource.Returned += OnActiveSourceReturned;
        
        var audioSource = activeSource.GetComponent<AudioSource>(); 

        audioSource.resource = audioResource;
        audioSource.outputAudioMixerGroup = AudioService.Instance.GetMixerGroupMapping(sfxCategory);
        audioSource.Play();
    }

    private void OnActiveSourceReturned(Poolable poolable)
    {
        if (activeSource == null) return;
        
        activeSource.Returned -= OnActiveSourceReturned;
        activeSource = null;
    }
}