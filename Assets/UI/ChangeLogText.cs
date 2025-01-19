using System.Linq;
using TMPro;
using UnityEngine;

public class ChangeLogText : MonoBehaviour
{
    [SerializeField] private Changelog changelog;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private GameObject content;

    public void ToggleVisibility()
    {
        content.SetActive(!content.activeSelf);
    }
    
    private void OnEnable()
    {
        changelog.LogUpdated += OnChangelogUpdated;
    }

    private void OnDisable()
    {
        changelog.LogUpdated -= OnChangelogUpdated;
    }

    private void OnChangelogUpdated()
    {
        string redoText = changelog.RedoLog.Count > 0
            ? $"\n{string.Join("\n", changelog.RedoLog.Reverse())}"
            : "";
        
        string undoText = changelog.UndoLog.Count > 0
            ? $"\n>> {string.Join("\n", changelog.UndoLog)}"
            : ""; 
        
        textMesh.text =
            $"<b><size=125%>Change Log</size></b>\n{redoText}{undoText}";
    }
}
