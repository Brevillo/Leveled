using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoonBar : MonoBehaviour
{
    [SerializeField] private float length;
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
    private Vector2 planetPerlinSeed;
    private float angle;

    private int MoonCount => Mathf.FloorToInt((length - firstMoonOffset) / moonSpacing);
    
    private void Start()
    {
        moons = Enumerable.Range(0, MoonCount)
            .Select(_ => Instantiate(moonPrefab, moonParent))
            .ToArray();

        moonPerlinSeeds = Enumerable.Range(0, MoonCount).Select(_ => Random.insideUnitCircle * 1000f).ToArray();

        planetPerlinSeed = Random.insideUnitCircle * 1000f;
    }

    private void Update()
    {
        angle = (angle + rotationSpeed * rotationDirection * Time.deltaTime) % 360f;

        float rads = angle * Mathf.Deg2Rad;

        Vector2 planetPosition = PerlinNoiseOffset(planetPerlinSeed, Time.time * planetWiggleSpeed, planetWiggleAmplitude);
        planetPivot.localPosition = planetPosition;
        
        for (int i = 0; i < MoonCount; i++)
        {
            var moonPerlinSeed = moonPerlinSeeds[i];
            float time = Time.time * moonWiggleSpeed;

            moons[i].transform.localPosition =
                new Vector2(Mathf.Cos(rads), Mathf.Sin(rads)) * (firstMoonOffset + i * moonSpacing)
                + PerlinNoiseOffset(moonPerlinSeed, time, moonWiggleAmplitude)
                + planetPosition;
        }
    }

    private static Vector2 PerlinNoiseOffset(Vector2 seed, float time, float amplitude) =>
        new Vector2(Mathf.PerlinNoise1D(seed.x + time), Mathf.PerlinNoise1D(seed.y + time)) * amplitude;
}
