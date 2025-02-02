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
    [SerializeField] private TextMeshProUGUI option1Label;
    [SerializeField] private TextMeshProUGUI option2Label;
    [SerializeField] private TextMeshProUGUI cancelLabel;
    
    [SerializeField] private GameObject primaryConfirmButton;
    [SerializeField] private GameObject secondaryConfirmButton;
    [SerializeField] private GameObject option1Button;
    [SerializeField] private GameObject option2Button;
    [SerializeField] private GameObject content;

    private Action primaryConfirmAction;
    private Action cancelAction;
    private Action secondaryConfirmAction;
    private Action option1Action;
    private Action option2Action;

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

    public void OpenDoubleOptionMenu(
        string menuTitle,
        string option1Text, string option2Text, string cancelText,
        Action option1Action,
        Action option2Action)
    {
        titleText.text = menuTitle;
        option1Label.text = option1Text;
        option2Label.text = option2Text;
        cancelLabel.text = cancelText;
        
        this.option1Action = option1Action;
        this.option2Action = option2Action;
        
        option1Button.SetActive(true);
        option2Button.SetActive(true);
        content.SetActive(true);
    }
    
    private void Awake()
    {
        DisableMenu();
    }

    public void PrimaryConfirm()
    {
        primaryConfirmAction?.Invoke();
        DisableMenu();
    }

    public void SecondaryConfirm()
    {
        secondaryConfirmAction?.Invoke();
        DisableMenu();
    }

    public void Option1()
    {
        option1Action?.Invoke();
        DisableMenu();
    }

    public void Option2()
    {
        option2Action?.Invoke();
        DisableMenu();
    }
    
    public void Canceled()
    {
        cancelAction?.Invoke();
        DisableMenu();
    }

    private void DisableMenu()
    {
        content.SetActive(false);
        
        secondaryConfirmButton.SetActive(false);
        primaryConfirmButton.SetActive(false);
        option1Button.SetActive(false);
        option2Button.SetActive(false);

        primaryConfirmAction = null;
        secondaryConfirmAction = null;
        cancelAction = null;
        option1Action = null;
        option2Action = null;
    }
}
