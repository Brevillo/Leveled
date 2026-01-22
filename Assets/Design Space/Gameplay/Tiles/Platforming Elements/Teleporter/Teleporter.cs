using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private UnityEvent onTeleport;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private TileEntity tileEntity;

    private Teleporter connection;
    
    private readonly List<Teleportable> ignore = new();

    private static readonly Dictionary<Vector2Int, Teleporter> teleporters = new();

    private void Awake()
    {
        tileEntity.Initialized += OnInitialized;
    }

    private void OnInitialized()
    {
        teleporters.Add(tileEntity.CellPosition, this);
        RecalculateAllConnections();
    }

    private void OnDestroy()
    {
        teleporters.Remove(tileEntity.CellPosition);
        RecalculateAllConnections();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (connection == null) return;
        
        if (other.TryGetComponent(out Teleportable teleportable) && !ignore.Remove(teleportable))
        {
            teleportable.Teleport(connection.transform.position);
            connection.ignore.Add(teleportable);
            onTeleport.Invoke();
        }
    }
    
    private void RecalculateAllConnections()
    {
        var connections = LinkingGroup.GetAllConnections(editorState.LevelInstance);
        
        foreach (var self in teleporters.Values)
        {
            self.connection = teleporters.GetValueOrDefault(connections[tileEntity.CellPosition]);
        }
    }
}
