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
    [SerializeField] private RectTransform screenRect;
    [SerializeField] private GameObject blocker;
    [SerializeField] private EditorButtonActions editorButtonActions;
    [SerializeField] private List<string> preGenerateOptions;
    
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

    public void GetLinkingGroupAtPosition(Vector2 viewportPosition, Action<string> linkingGroupAction)
    {
        editorButtonActions.enabled = false;
        
        windowPosition.gameObject.SetActive(true);

        Vector2 position = screenRect.rect.size * (viewportPosition - Vector2.one / 2f);
        Vector2 max = screenRect.rect.size / 2f - windowPosition.rect.size * (Vector2.one - windowPosition.pivot);
        Vector2 min = screenRect.rect.size / 2f - windowPosition.rect.size * windowPosition.pivot;
        position.x = Mathf.Clamp(position.x, -min.x, max.x);
        position.y = Mathf.Clamp(position.y, -min.y, max.y);
        
        windowPosition.anchoredPosition = position;
        
        this.linkingGroupAction = linkingGroupAction;

        inputField.text = "";
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        
        blocker.SetActive(true);
        
        foreach (var linkingGroup in editorState.LinkingGroups
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

        editorButtonActions.enabled = true;

        linkingGroupAction?.Invoke(linkingGroup);
        linkingGroupAction = null;
        
        windowPosition.gameObject.SetActive(false);

        blocker.SetActive(false);
        
        foreach (var option in spawnedOptions)
        {
            Destroy(option);
        }
        
        spawnedOptions.Clear();
    }
}
