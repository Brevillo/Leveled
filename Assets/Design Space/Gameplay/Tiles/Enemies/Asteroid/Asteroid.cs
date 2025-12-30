using System.Linq;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private float rotationDuration;
    [SerializeField] private float rotationRadius;
    [SerializeField] private float rotationAnchor;
    [SerializeField] private int rotationDirection;
    [SerializeField] private Transform projectilePivot;
    [SerializeField] private float hitboxDuration;
    [SerializeField] private GameObject hitbox;
    [SerializeField] private LineRenderer frontLine;
    [SerializeField] private LineRenderer backLine;
    [SerializeField] private float lineAngleRange;
    [SerializeField] private int pointsPerLine;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private string frontLayer;
    [SerializeField] private string backLayer;
    [SerializeField] private AnimationCurve sizeOverRangeCurve;
    
    private float rotationTimer;

    private void Start()
    {
        frontLine.positionCount = pointsPerLine;
        frontLine.SetPositions(GetPoints(-lineAngleRange / 2f, 0f));
        
        backLine.positionCount = pointsPerLine;
        backLine.SetPositions(GetPoints(0f, lineAngleRange / 2f));

        Vector3[] GetPoints(float start, float end) => Enumerable.Range(0, pointsPerLine)
            .Select(i =>
            {
                float rads = Mathf.Lerp(start, end, (float)i / (pointsPerLine - 1)) * Mathf.Deg2Rad;
                return new Vector3(0f, Mathf.Cos(rads) - 1f, Mathf.Sin(rads)) * rotationRadius;
            })
            .ToArray();
    }

    private void Update()
    {
        rotationTimer = (rotationTimer + Time.deltaTime) % rotationDuration;

        hitbox.SetActive(rotationTimer < hitboxDuration);

        float rotationPercent = rotationTimer / rotationDuration;

        sprite.sortingLayerName = (rotationPercent - 0.5f) * rotationDirection > 0f
            ? frontLayer
            : backLayer;

        float sizePercent = 1f - Mathf.InverseLerp(0f, lineAngleRange / 360f, 1f - Mathf.Abs(1f - 2f * rotationPercent));
        projectilePivot.localScale = Vector3.one * sizeOverRangeCurve.Evaluate(sizePercent);

        float rads = (rotationAnchor + rotationPercent * 360f * rotationDirection) * Mathf.Deg2Rad;
        projectilePivot.localPosition = new Vector3(0f, Mathf.Cos(rads) - 1f, Mathf.Sin(rads)) * rotationRadius;
    }
}
