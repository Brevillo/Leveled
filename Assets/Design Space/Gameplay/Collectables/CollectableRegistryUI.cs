using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectableRegistryUI : MonoBehaviour
{
    [Header("UI")] 
    [SerializeField] private Color regularColor;
    [SerializeField] private Color allCollectedColor;
    [SerializeField] private List<Graphic> graphics;
    [SerializeField] private TextMeshProUGUI collected;
    [SerializeField] private GameObject content;
    
    [Header("Registration")]
    [SerializeField] private CollectableRegistry registry;
    [SerializeField] private GameStateManager gameStateManager;

    private void OnEnable()
    {
        registry.Collected += UpdateUI;
        registry.CountChanged += UpdateVisibility;
        gameStateManager.EditorStateChanged += OnEditorStateChanged;
    }

    private void OnDisable()
    {
        registry.Collected -= UpdateUI;
        registry.CountChanged -= UpdateVisibility;
        gameStateManager.EditorStateChanged -= OnEditorStateChanged;
    }

    private void UpdateVisibility()
    {
        content.SetActive(gameStateManager.EditorState == EditorState.Playing && registry.TotalCount > 0);
        UpdateUI();
    }

    private void UpdateUI()
    {
        collected.text = registry.CollectedCount.ToString();
        
        var graphicColor = registry.AllCollected
            ? allCollectedColor
            : regularColor;

        graphics.ForEach(graphic => graphic.color = graphicColor);
    }

    private void OnEditorStateChanged(EditorState editorState)
    {
        UpdateVisibility();
    }
}
