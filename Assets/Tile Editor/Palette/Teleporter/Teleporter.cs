using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private SpriteRenderer arrow;

    private Teleporter connection;
    private string linkingGroup;
    private bool disable;
    
    private static readonly List<Teleporter> teleporters = new();
    private static event Action ConnectionsRecalculated;
    
    private void OnConnectionsRecalculated()
    {
        if (connection != null && editorState.ShowLinkingGroups)
        {
            Vector2 self = transform.position + Vector3.up * 0.5f;
            Vector2 other = connection.transform.position + Vector3.down * 0.5f;
            float distance = Vector2.Distance(self, other);
            other = self + (other - self) / distance * (distance - 0.5f);
            
            arrow.size = new(Vector3.Distance(other, self) / arrow.transform.localScale.x, 1);
            arrow.transform.SetPositionAndRotation(
                (self + other) / 2f,
                Quaternion.FromToRotation(Vector2.right, other - self));
            
            arrow.gameObject.SetActive(true);
        }
        else
        {
            arrow.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        editorState.EditorChanged += OnEditorChanged;
        ConnectionsRecalculated += OnConnectionsRecalculated;
        
        teleporters.Add(this);
        RecalculateAllConnections();
    }
    
    private void OnDisable()
    {
        editorState.EditorChanged -= OnEditorChanged;
        ConnectionsRecalculated -= OnConnectionsRecalculated;
        
        teleporters.Remove(this);
        RecalculateAllConnections();
    }

    private void Start()
    {
        RecalculateAllConnections();
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case ShowLinkingGroupsChangeInfo showLinkingGroupsChangeInfo:
                
                arrow.gameObject.SetActive(showLinkingGroupsChangeInfo.newValue);
                RecalculateAllConnections();
                
                break;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (disable)
        {
            disable = false;
            return;
        }

        if (connection == null) return;
        
        if (other.TryGetComponent(out Teleportable teleportable))
        {
            teleportable.Teleport(connection.transform.position);
            connection.disable = true;
        }
    }

    private void RecalculateAllConnections()
    {
        foreach (var teleporter in teleporters)
        {
            teleporter.linkingGroup = editorState.GetLinkingGroup(teleporter.gameObject);
        }
        
        foreach (var self in teleporters)
        {
            self.connection = teleporters
                .Where(teleporter => teleporter.linkingGroup == self.linkingGroup && teleporter != self)
                .OrderBy(teleporter => Vector3.Distance(teleporter.transform.position, self.transform.position))
                .FirstOrDefault();
        }
        
        ConnectionsRecalculated?.Invoke();
    }
}
