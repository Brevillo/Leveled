using System;
using System.Collections.Generic;
using System.Linq;
using OliverBeebe.UnityUtilities.Runtime.Pooling;
using TMPro;
using UnityEngine;

public class ChangeLogWindowUI : MonoBehaviour
{
    [SerializeField] private Changelog changelog;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private GameObject windowContent;
    [SerializeField] private Transform entriesParent;
    [SerializeField] private GameObjectPool entryItemPool;
    [SerializeField] private PanelResizer panelResizer;

    private Stack<ChangelogEntryItem> undoEntries = new(), redoEntries = new();

    private ChangelogEntryItem activeEntry;

    private bool visible;
    
    public void ToggleVisibility()
    {
        visible = !visible;
        windowContent.SetActive(visible);
    }

    public void CloseChangelog()
    {
        visible = false;
        windowContent.SetActive(false);
    }
    
    private void Awake()
    {
        panelResizer.PanelToggled += OnPanelToggled;
    }

    private void OnPanelToggled(bool open)
    {
        windowContent.SetActive(open && visible);
    }

    private void OnEnable()
    {
        changelog.LogUpdated += OnChangelogUpdated;
    }

    private void OnDisable()
    {
        changelog.LogUpdated -= OnChangelogUpdated;
    }

    private void AddEntry(string text)
    {
        var entryItem = entryItemPool.Retrieve(entriesParent).GetComponent<ChangelogEntryItem>();
        
        activeEntry = entryItem;
        activeEntry.transform.SetSiblingIndex(0);
        activeEntry.Setup(text, OnEntryClicked);

        void OnEntryClicked()
        {
            int undoIndex = undoEntries.ToList().IndexOf(entryItem);

            if (undoIndex != -1)
            {
                changelog.Undo(undoIndex + 1);
                return;
            }

            int redoIndex = redoEntries.ToList().IndexOf(entryItem);

            if (redoIndex != -1)
            {
                changelog.Redo(redoIndex + 1);
                return;
            }
        }
    }
    
    private void OnChangelogUpdated(Changelog.LogUpdateType updateType)
    {
        if (activeEntry != null)
        {
            activeEntry.SetActive(false);
        }
        
        switch (updateType)
        {
            case Changelog.LogUpdateType.Cleared:

                if (activeEntry != null)
                {
                    activeEntry.Return();
                }
                
                foreach (var entry in undoEntries)
                {
                    entry.Return();
                }
                undoEntries.Clear();

                foreach (var entry in redoEntries)
                {
                    entry.Return();
                }
                redoEntries.Clear();

                AddEntry("Original State");
                
                break;

            case Changelog.LogUpdateType.Undo: 
            {
                redoEntries.Push(activeEntry);
                activeEntry = undoEntries.Pop();
            }
                break;

            case Changelog.LogUpdateType.Redo:
            {
                undoEntries.Push(activeEntry);
                activeEntry = redoEntries.Pop();
            }
                break;

            case Changelog.LogUpdateType.NewChange:
            {
                if (activeEntry != null)
                {
                    undoEntries.Push(activeEntry);
                }
                
                foreach (var entry in redoEntries)
                {
                    entry.Return();
                }
                
                redoEntries.Clear();
                AddEntry(changelog.UndoLog.First().ToString());
            }
                break;
        }

        if (activeEntry != null)
        {
            activeEntry.SetActive(true);
        }
    }
}
