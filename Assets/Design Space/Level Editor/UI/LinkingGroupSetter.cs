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

    private Action<LinkingGroup> linkingGroupAction;
    
    private void Awake()
    {
        inputField.onSubmit.AddListener(input => ChooseOption(new(input)));
        spawnedOptions = new();

        foreach (var linkingGroup in preGenerateOptions)
        {
            SpawnOption(new(linkingGroup));
        }
    }

    public void GetLinkingGroupAtMouse(Action<LinkingGroup> linkingGroupAction) =>
        GetLinkingGroup(spaceUtility.MouseCellCenterWorld + Vector3.down * 0.5f, linkingGroupAction);
    
    public void GetLinkingGroup(Vector2 worldPosition, Action<LinkingGroup> linkingGroupAction)
    {
        content.SetActive(true);
        
        Vector2 canvasPosition = (Vector2)spaceUtility.WorldToCanvas(worldPosition, windowPosition) + windowOffset;
        windowPosition.position =
            spaceUtility.ClampCanvasPointToCanvasRect(canvasPosition, windowPosition, screenEdgeBuffer);
        
        this.linkingGroupAction = linkingGroupAction;

        inputField.text = "";
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        
        foreach (var linkingGroupID in editorState.LevelInstance
                     .GetAllMetadata<LinkingGroup>()
                     .GroupBy(group => group.groupID)
                     .Select(group => group.Key)
                     .Where(groupID => !preGenerateOptions.Exists(option => option == groupID)))
        {
            spawnedOptions.Add(SpawnOption(new(linkingGroupID)));
        }
    }

    private GameObject SpawnOption(LinkingGroup linkingGroup)
    {
        var option = Instantiate(optionPrefab, optionsParent);

        option.GetComponentInChildren<TextMeshProUGUI>().text = linkingGroup.groupID;
        option.GetComponentInChildren<Button>().onClick.AddListener(() => ChooseOption(linkingGroup));

        return option;
    }

    private void ChooseOption(LinkingGroup linkingGroup)
    {
        if (linkingGroup.groupID == default) return;
        
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
