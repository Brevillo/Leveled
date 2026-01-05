using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LinkingGroupSetter : MonoBehaviour
{
    [SerializeField] private GameObject optionPrefab;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform optionsParent;
    [SerializeField] private TileEditorState editorState;
    [SerializeField] private RectTransform windowPosition;
    [SerializeField] private SpaceUtility spaceUtility;
    [SerializeField] private GameObject content;
    [SerializeField] private List<string> preGenerateOptions;
    [SerializeField] private Vector2 windowOffset;
    [SerializeField] private Vector2 screenEdgeBuffer;
    [SerializeField] private GameStateManager gameStateManager;
    
    private List<GameObject> spawnedOptions;

    private Action<string> linkingGroupAction;
    
    private void Awake()
    {
        inputField.onSubmit.AddListener(ChooseOption);
        spawnedOptions = new();

        foreach (var linkingGroup in preGenerateOptions)
        {
            SpawnOption(linkingGroup);
        }
    }

    public void GetLinkingGroupAtMouse(Action<string> linkingGroupAction) =>
        GetLinkingGroup(spaceUtility.MouseCellCenterWorld + Vector3.down * 0.5f, linkingGroupAction);
    public void GetLinkingGroup(Vector2 worldPosition, Action<string> linkingGroupAction)
    {
        content.SetActive(true);
        
        Vector2 canvasPosition = (Vector2)spaceUtility.WorldToCanvas(worldPosition, windowPosition) + windowOffset;
        windowPosition.position =
            spaceUtility.ClampCanvasPointToCanvasRect(canvasPosition, windowPosition, screenEdgeBuffer);
        
        this.linkingGroupAction = linkingGroupAction;

        inputField.text = "";
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        
        foreach (var linkingGroup in editorState.TileData
                     .GroupBy(tile => tile.linkingGroup)
                     .Select(group => group.Key)
                     .Where(group => !preGenerateOptions.Contains(group)))
        {
            spawnedOptions.Add(SpawnOption(linkingGroup));
        }
    }

    private GameObject SpawnOption(string linkingGroup)
    {
        var option = Instantiate(optionPrefab, optionsParent);

        option.GetComponentInChildren<TextMeshProUGUI>().text = linkingGroup;
        option.GetComponentInChildren<Button>().onClick.AddListener(() => ChooseOption(linkingGroup));

        return option;
    }

    private void ChooseOption(string linkingGroup)
    {
        if (linkingGroup == "") return;
        
        content.SetActive(false);
        gameStateManager.EnterEditMode();
        
        linkingGroupAction?.Invoke(linkingGroup);
        linkingGroupAction = null;
        
        foreach (var option in spawnedOptions)
        {
            Destroy(option);
        }
        
        spawnedOptions.Clear();
    }
}
