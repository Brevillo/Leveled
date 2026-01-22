using System;
using TMPro;
using UnityEngine;

public class SignDataViewportVisual : MonoBehaviour
{
    [SerializeField] private MetadataViewportVisual metadataViewportVisual;
    [SerializeField] private TextMeshProUGUI textDisplay;

    private void Awake()
    {
        metadataViewportVisual.MetadataUpdated += OnMetadataUpdated;
    }

    private void OnMetadataUpdated(object metadata)
    {
        textDisplay.text = ((SignData)metadata).text;
    }
}
