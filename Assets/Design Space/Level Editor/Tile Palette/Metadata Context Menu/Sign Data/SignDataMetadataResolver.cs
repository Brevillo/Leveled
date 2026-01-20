using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = CreateAssetMenuPath + "Sign Data")]
public class SignDataMetadataResolver : MetadataResolver
{
    [SerializeField] private bool defaultShowInGame;
    [SerializeField] private string defaultText;
    
    public override object GetCurrentValue(Vector2Int[] selection, TileEditorState tileEditorState)
    {
        var allSignData = selection
            .Select(position => tileEditorState.LevelInstance.GetTileOnAnyLayer(position).GetMetaData<SignData>())
            .ToArray(); 
        
        var selectedSignText = allSignData
            .GroupBy(signData => signData.text)
            .Select(group => group.Key)
            .Where(text => !string.IsNullOrEmpty(text))
            .DefaultIfEmpty(defaultText)
            .First();

        var selectedShowInGame = allSignData
            .GroupBy(signData => signData.showInGame)
            .Select(group => group.Key)
            .DefaultIfEmpty(defaultShowInGame)
            .First();

        return new SignData(selectedSignText, selectedShowInGame);
    }
}
