using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectedTilesUI : MonoBehaviour
{
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private Image primaryTileIcon;
    [SerializeField] private Image secondaryTileIcon;
    [SerializeField] private Sprite noTileSprite;

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
        switch (changeInfo)
        {
            case PaletteChangeInfo paletteChangeInfo:

                var icon = paletteChangeInfo.type switch
                {
                    PaletteChangeInfo.Type.Secondary => secondaryTileIcon,
                    PaletteChangeInfo.Type.Primary or _ => primaryTileIcon,
                };

                icon.sprite = paletteChangeInfo.newValue == null 
                    ? noTileSprite
                    : paletteChangeInfo.newValue.PaletteIcon;
                
                break;
        }
    }
}
