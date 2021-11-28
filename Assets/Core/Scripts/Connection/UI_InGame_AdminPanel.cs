using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumsAndData;

public class UI_InGame_AdminPanel : MonoBehaviour
{
    public static UI_InGame_AdminPanel instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
    }

    [Header("AdminPanelRelated")]
    public GameObject adminPanelButton;
    public GameObject adminPanel;

    void Start()
    {
        ManageAdminAccessBtn();
    }

    void ManageAdminAccessBtn()
    {
        if (ConnectionManager.instance.currentUserData.IsAdmin())
            adminPanelButton.SetActive(true); 
        else adminPanelButton.SetActive(false);

        adminPanel.gameObject.SetActive(false);
    }

    public void OnClick_OpenAdminPanel()
    {
        if (ConnectionManager.instance.currentUserData.IsAdmin())
        {
            adminPanel.SetActive(true);
            UI_InGame.instance.AdminPanelOpened(true);
        }
    }

    public void OnClick_CloseAdminPanel()
    {
        adminPanel.SetActive(false);
        UI_InGame.instance.AdminPanelOpened(false);
    }
}
