using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoonBar : MonoBehaviour
{
    [SerializeField] private int moonCount;
    [SerializeField] private int rotationDirection;
    [Space]
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float moonSpacing;
    [SerializeField] private float firstMoonOffset;
    [SerializeField] private float moonWiggleSpeed;
    [SerializeField] private float moonWiggleAmplitude;
    [SerializeField] private SpriteRenderer moonPrefab;
    [SerializeField] private Transform moonParent;
    [SerializeField] private float planetWiggleSpeed;
    [SerializeField] private float planetWiggleAmplitude;
    [SerializeField] private Transform planetPivot;

    private SpriteRenderer[] moons;
    private Vector2[] moonPerlinSeeds;
    private float angle;

    private void Start()
    {
        moons = Enumerable.Range(0, moonCount)
            .Select(_ => Instantiate(moonPrefab, moonParent))
            .ToArray();

        moonPerlinSeeds = Enumerable.Range(0, moonCount).Select(_ => Random.insideUnitCircle * 1000f).ToArray();
    }

    private void Update()
    {
        angle = (angle + rotationSpeed * rotationDirection * Time.deltaTime) % 360f;

        float rads = angle * Mathf.Deg2Rad;
        
        for (int i = 0; i < moonCount; i++)
        {
            var moonPerlinSeed = moonPerlinSeeds[i];
            float time = Time.time * moonWiggleSpeed;

            moons[i].transform.localPosition =
                new Vector2(Mathf.Cos(rads), Mathf.Sin(rads)) * (firstMoonOffset + i * moonSpacing)
                + new Vector2(Mathf.PerlinNoise1D(moonPerlinSeed.x + time), Mathf.PerlinNoise1D(moonPerlinSeed.y + time)) * moonWiggleAmplitude;
        }

        planetPivot.localPosition = Vector2.up * Mathf.Sin(Time.time * planetWiggleSpeed) * planetWiggleAmplitude;
    }
}
