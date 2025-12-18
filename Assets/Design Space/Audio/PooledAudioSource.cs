using OliverBeebe.UnityUtilities.Runtime.Pooling;
using UnityEngine;

public class PooledAudioSource : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Poolable poolable;

    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            poolable.Return();
        }
    }
}