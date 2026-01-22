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
    private bool active;

    private PhysicsContactParent[] physicsContactParents;

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

                foreach (var physicsContactParent in physicsContactParents)
                {
                    physicsContactParent.ChildAdded -= OnChildAdded;
                }
                
                break;

            case EditorState.Playing:

                movePercent = 0f;
                direction = 1;
                currentPointIndex = 0;
                active = false;

                physicsContactParents = GetComponentsInChildren<PhysicsContactParent>();
                
                foreach (var physicsContactParent in physicsContactParents)
                {
                    physicsContactParent.ChildAdded += OnChildAdded;
                }
                
                break;
        }
    }

    private void OnChildAdded(PhysicsContactChild child)
    {
        if (!levelLayer.Metadata.TryGetValue(out PathInstance pathInstance) ||
            pathInstance.activationType is not PathInstance.ActivationType.TouchStart) return;
        
        if (child.TryGetComponent(out PlayerMovement _))
        {
            active = true;
        }
    }

    private void FixedUpdate()
    {
        if (gameStateManager.EditorState != EditorState.Playing 
            || !levelLayer.Metadata.TryGetValue(out PathInstance pathInstance)
            || pathInstance.points.Count == 0
            || (!active && pathInstance.activationType == PathInstance.ActivationType.TouchStart))
        {
            return;
        }

        var points = pathInstance.points;
        var type = pathInstance.loopingType;
        
        Vector2 current = spaceUtility.CellToWorld(points[currentPointIndex]);
        Vector2 next = spaceUtility.CellToWorld(points[type switch
        {
            PathInstance.LoopingType.PingPong => Mathf.Clamp(currentPointIndex + direction, 0, points.Count - 1),
            PathInstance.LoopingType.Forward => (currentPointIndex + 1) % points.Count,
            PathInstance.LoopingType.Once => Mathf.Min(currentPointIndex + 1, points.Count - 1),
            _ => throw new ArgumentOutOfRangeException(),
        }]);

        float segmentDuration = Vector2.Distance(current, next) / moveSpeed;
        movePercent += Time.deltaTime / segmentDuration;
        
        Vector2 origin = spaceUtility.CellToWorld(points[0]);
        transform.position = Vector2.Lerp(current, next, movePercent) - origin;

        if (movePercent >= 1f)
        {
            movePercent = 0f;

            currentPointIndex += direction;
            
            switch (type)
            {
                case PathInstance.LoopingType.PingPong:

                    if (currentPointIndex == points.Count - 1 || currentPointIndex == 0)
                    {
                        direction *= -1;
                    }
                    
                    break;
                
                case PathInstance.LoopingType.Forward:

                    currentPointIndex %= points.Count;
                    
                    break;
                
                case PathInstance.LoopingType.Once:

                    currentPointIndex = Mathf.Min(currentPointIndex, points.Count - 1);
                    
                    break;
            }
        }
    }
}