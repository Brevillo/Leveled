using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private SpriteRenderer arrow;
    [SerializeField] private UnityEvent onTeleport;
    [SerializeField] private ChangeloggedBool showLinkingGroups;
    [SerializeField] private Changelog changelog;
    [SerializeField] private SpaceUtility spaceUtility;

    private Teleporter connection;
    private string linkingGroup;

    private List<Teleportable> ignore = new();
    
    private static readonly List<Teleporter> teleporters = new();
    
    private void OnEnable()
    {
        changelog.ChangeEvent += OnChangeEvent;
        
        teleporters.Add(this);
        RecalculateAllConnections();
    }
    
    private void OnDisable()
    {
        changelog.ChangeEvent -= OnChangeEvent;

        teleporters.Remove(this);
        RecalculateAllConnections();
    }

    private void OnChangeEvent(ChangeInfo changeInfo)
    {
        arrow.gameObject.SetActive(showLinkingGroups.Value);
        
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
        foreach (var teleporter in teleporters)
        {
            teleporter.linkingGroup = editorState.Level.GetTile(spaceUtility.WorldToCell(teleporter.transform.position))
                .linkingGroup;
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
        if (connection != null && showLinkingGroups.Value)
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
