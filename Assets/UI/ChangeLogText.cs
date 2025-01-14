using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class ChangeLogText : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private TextMeshProUGUI textMesh;

    private void OnEnable()
    {
        editorState.EditorChanged += OnEditorChanged;
    }

    private void OnDisable()
    {
        editorState.EditorChanged -= OnEditorChanged;
    }

    private void OnEditorChanged(ChangeInfo changeInfo)
    {
        string redoText = editorState.RedoLog.Count > 0
            ? $"\n{string.Join("\n", editorState.RedoLog.Reverse())}"
            : "";
        
        string undoText = editorState.UndoLog.Count > 0
            ? $"\n>> {string.Join("\n", editorState.UndoLog)}"
            : ""; 
        
        textMesh.text =
            $"<b><size=125%>Change Log</size></b>\n{redoText}{undoText}";
    }
}
