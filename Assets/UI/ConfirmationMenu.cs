using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ConfirmationMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI primaryConfirmLabel;
    [SerializeField] private TextMeshProUGUI secondaryConfirmLabel;
    [SerializeField] private TextMeshProUGUI cancelLabel;
    [SerializeField] private GameObject primaryConfirmButton;
    [SerializeField] private GameObject secondaryConfirmButton;
    [SerializeField] private GameObject content;

    private Action primaryConfirmAction;
    private Action cancelAction;
    private Action secondaryConfirmAction;

    public void OpenDestructiveConfirmMenu(
        string menuTitle,
        string confirmText, string cancelText,
        Action confirmAction)
    {
        secondaryConfirmAction = confirmAction;

        titleText.text = menuTitle;
        secondaryConfirmLabel.text = confirmText;
        cancelLabel.text = cancelText;
        
        secondaryConfirmButton.SetActive(true);
        primaryConfirmButton.SetActive(false);
        content.SetActive(true);
    }
    
    public void OpenOptionalConfirmMenu(
        string menuTitle,
        string primaryConfirmText, string secondaryConfirmText, string cancelText,
        Action primaryConfirmAction, 
        Action secondaryConfirmAction = null)
    {
        titleText.text = menuTitle;
        primaryConfirmLabel.text = primaryConfirmText;
        secondaryConfirmLabel.text = secondaryConfirmText;
        cancelLabel.text = cancelText;
        
        this.primaryConfirmAction = primaryConfirmAction;
        this.secondaryConfirmAction = secondaryConfirmAction;
        
        primaryConfirmButton.SetActive(true);
        secondaryConfirmButton.SetActive(secondaryConfirmAction != null);
        content.SetActive(true);
    }

    private void Awake()
    {
        content.SetActive(false);
    }

    public void PrimaryConfirm()
    {
        primaryConfirmAction?.Invoke();
        
        content.SetActive(false);
    }

    public void SecondaryConfirm()
    {
        secondaryConfirmAction?.Invoke();
        
        content.SetActive(false);
    }
    
    public void Canceled()
    {
        cancelAction?.Invoke();
        
        content.SetActive(false);
    }
}
