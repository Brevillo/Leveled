using System;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using UnityEngine;
using UnityEngine.UI;

public class SliderSteps : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject selectedStepPrefab, unselectedStepPrefab;
    [SerializeField] private Transform stepsParent;

    private ObjectPool<GameObject> selectedSteps, unselectedSteps;
    
    private void Awake()
    {
        selectedSteps = new();
        unselectedSteps = new();
        
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void Start()
    {
        OnSliderValueChanged(slider.value);
    }

    private void OnSliderValueChanged(float value)
    {
        foreach (var step in selectedSteps.ActiveObjects)
        {
            step.SetActive(false);
        }
        
        selectedSteps.ReturnAll();

        foreach (var step in unselectedSteps.ActiveObjects)
        {
            step.SetActive(false);
        }
        
        unselectedSteps.ReturnAll();

        for (int i = 0; i < value - slider.minValue; i++)
        {
            var step = selectedSteps.Retrieve(CreateStep(selectedStepPrefab));
            step.SetActive(true);
            step.transform.SetAsFirstSibling();
        }

        for (int i = 0; i < slider.maxValue - value; i++)
        {
            var step = unselectedSteps.Retrieve(CreateStep(unselectedStepPrefab));
            step.SetActive(true);
            step.transform.SetAsLastSibling();
        }
    }

    private Func<GameObject> CreateStep(GameObject stepPrefab) => () => Instantiate(stepPrefab, stepsParent);
}
