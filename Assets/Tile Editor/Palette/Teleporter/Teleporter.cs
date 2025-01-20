using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;

    private string linkingGroup;
    private Teleporter connection;
    private bool disable;
    
    private static readonly List<Teleporter> teleporters = new();
    private static event Action RecalculateConnections;
    
    private void Awake()
    {
        linkingGroup = editorState.GetLinkingGroup(gameObject);
    }

    private void OnRecalculateConnections()
    {
        connection = teleporters
            .Where(teleporter => teleporter.linkingGroup == linkingGroup && teleporter != this)
            .OrderBy(teleporter => Vector3.Distance(teleporter.transform.position, transform.position))
            .FirstOrDefault();
    }

    private void OnEnable()
    {
        RecalculateConnections += OnRecalculateConnections;
        
        teleporters.Add(this);
        RecalculateConnections?.Invoke();
    }

    private void OnDisable()
    {
        teleporters.Remove(this);
        RecalculateConnections?.Invoke();
        
        RecalculateConnections -= OnRecalculateConnections;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (disable)
        {
            disable = false;
            return;
        }
        
        if (other.TryGetComponent(out Teleportable teleportable))
        {
            teleportable.Teleport(connection.transform.position);
            connection.disable = true;
        }
    }
}
