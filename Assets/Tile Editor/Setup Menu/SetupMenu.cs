using System;
using UnityEngine;

public class SetupMenu : MonoBehaviour
{
    [SerializeField] private LeveledSaveSystem saveSystem;
    [SerializeField] private GameObject content;
    
    private void Update()
    {
        content.SetActive(!saveSystem.FolderChosen);
    }
}
