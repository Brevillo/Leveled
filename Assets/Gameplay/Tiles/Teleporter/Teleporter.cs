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
    private List<Teleportable> ignore = new();
    private ChangeInfo lastChangeInfo;
    
    private static readonly List<Teleporter> teleporters = new();
    
    private void OnEnable()
    {
        editorState.EditorChanged += OnEditorChanged;
        
        teleporters.Add(this);
        RecalculateAllConnections(editorState);
    }
    
    private void OnDisable()
    {
        editorState.EditorChanged -= OnEditorChanged;
        
        teleporters.Remove(this);
        RecalculateAllConnections(editorState);
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        switch (changeInfo)
        {
            case ShowLinkingGroupsChangeInfo showLinkingGroupsChangeInfo:
                
                arrow.gameObject.SetActive(showLinkingGroupsChangeInfo.newValue);
                RecalculateAllConnections(editorState);
                
                break;
            
            case TileChangeInfo:

                if (changeInfo != lastChangeInfo)
                {
                    lastChangeInfo = changeInfo;
                    RecalculateAllConnections(editorState);
                }
                break;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (connection == null) return;
        
        if (other.TryGetComponent(out Teleportable teleportable) && !ignore.Remove(teleportable))
        {
            teleportable.Teleport(connection.transform.position);
            connection.ignore.Add(teleportable);
        }
    }

    private static void RecalculateAllConnections(TileEditorState editorState)
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
            
            self.UpdateConnectionArrow();
        }
    }    
    
    private void UpdateConnectionArrow()
    {
        if (connection != null && editorState.ShowLinkingGroups)
        {
            Vector2 self = transform.position + Vector3.up * 0.5f;
            Vector2 other = connection.transform.position + Vector3.down * 0.5f;
            float distance = Vector2.Distance(self, other);
            
            other = distance == 0
                ? other
                : self + (other - self) / distance * (distance - 0.5f);
            
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
}
