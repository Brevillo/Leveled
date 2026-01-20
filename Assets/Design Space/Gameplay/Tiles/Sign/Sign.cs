using System;
using TMPro;
using UnityEngine;

public class Sign : MonoBehaviour
{
    [SerializeField] private TileEditorState tileEditorState;
    [SerializeField] private CanvasGroup uiContent;
    [SerializeField] private TextMeshProUGUI textDisplay;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private CollisionAggregate2D touching;
    [SerializeField] private TileEntity tileEntity;
    
    private SignData signData;
    
    private void Start()
    {
        var cellPosition = spaceUtility.WorldToCell(transform.position);

        signData = tileEditorState.LevelInstance.GetTile(cellPosition, tileEntity.layerID)
            .GetMetaData<SignData>();
        
        textDisplay.text = signData.text;
    }

    private void Update()
    {
        float alphaTarget = touching.Touching ? 1f : 0f;

        uiContent.alpha = Mathf.MoveTowards(
            uiContent.alpha,
            alphaTarget,
            Time.deltaTime / fadeInDuration);
    }
}

public readonly struct SignData
{
    public readonly string text;
    public readonly bool showInGame;
    
    public SignData(string text, bool showInGame)
    {
        this.text = text;
        this.showInGame = showInGame;
    }
}
