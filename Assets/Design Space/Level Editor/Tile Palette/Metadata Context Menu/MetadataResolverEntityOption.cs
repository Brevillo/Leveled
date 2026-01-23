using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MetadataResolverEntityOption : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameDisplay;
    [SerializeField] private Image iconDisplay;
    [SerializeField] private Button button;
    [SerializeField] private string nullEntityName;
    [SerializeField] private Sprite nullEntityIcon;

    private GameTile gameTile;
    
    public GameTile GameTile
    {
        get => gameTile;
        set
        {
            gameTile = value;

            (nameDisplay.text, iconDisplay.sprite) = gameTile != null 
                ? (gameTile.Tooltip, gameTile.PaletteIcon)
                : (nullEntityName, nullEntityIcon);
        }
    }

    public UnityEvent OnClick => button.onClick;
}