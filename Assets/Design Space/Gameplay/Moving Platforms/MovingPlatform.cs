using System;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private LevelLayer levelLayer;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private float moveSpeed;
    [SerializeField] private SpaceUtility spaceUtility;
    
    private int currentPointIndex;
    private int direction = 1;
    private float movePercent;

    private void Awake()
    {
        gameStateManager.EditorStateChanged += OnEditorStateChanged;
    }

    private void OnDestroy()
    {
        gameStateManager.EditorStateChanged -= OnEditorStateChanged;
    }

    private void OnEditorStateChanged(EditorState editorState)
    {
        switch (editorState)
        {
            case EditorState.Editing:

                transform.localPosition = Vector3.zero;
                
                break;
            
            case EditorState.Playing:
                
                movePercent = 0f;
                direction = 1;
                currentPointIndex = 0;
                
                break;
        }
    }

    private void FixedUpdate()
    {
        if (gameStateManager.EditorState != EditorState.Playing 
            || !levelLayer.Metadata.TryGetValue(out PathInstance pathInstance)
            || pathInstance.points.Count == 0)
        {
            return;
        }

        var points = pathInstance.points;
        var type = pathInstance.pathingType;

        Vector2Int origin = points[0];
        
        Vector2 current = spaceUtility.CellToWorld(points[currentPointIndex] - origin);
        Vector2 next = spaceUtility.CellToWorld(points[type switch
        {
            PathInstance.PathingType.PingPong => Mathf.Clamp(currentPointIndex + direction, 0, points.Count - 1),
            PathInstance.PathingType.Forward => (currentPointIndex + 1) % points.Count,
            PathInstance.PathingType.Once => Mathf.Min(currentPointIndex + 1, points.Count - 1),
            _ => throw new ArgumentOutOfRangeException(),
        }] - origin);

        float segmentDuration = Vector2.Distance(current, next) / moveSpeed;
        movePercent += Time.deltaTime / segmentDuration;

        transform.position = Vector2.Lerp(current, next, movePercent);

        if (movePercent >= 1f)
        {
            movePercent = 0f;

            currentPointIndex++;
            
            switch (type)
            {
                case PathInstance.PathingType.PingPong:

                    if (currentPointIndex == points.Count)
                    {
                        currentPointIndex--;
                        direction *= -1;
                    }
                    
                    break;
                
                case PathInstance.PathingType.Forward:

                    currentPointIndex %= points.Count;
                    
                    break;
                
                case PathInstance.PathingType.Once:

                    currentPointIndex = Mathf.Min(currentPointIndex, points.Count - 1);
                    
                    break;
            }
        }
    }
}