using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_InGame : MonoBehaviour
{
    public GameObject panel_inGame;
    public GameObject panel_Menu;

    public TMP_Text txt_currentLobbyName;

    private void Start()
    {
        OnClick_CloseMenu();
        OnlineGameManager.instance.ui_PlayersInLobby_Manager.SpawnLobbyItems(OnlineGameManager.currentPlayersScores_OnEnter);
        txt_currentLobbyName.text = OnlineGameManager.currentLobbyName_OnEnter;
    }

    public void OnClick_OpenMenu()
    {
        panel_inGame.SetActive(false);
        panel_Menu.SetActive(true);
    }
    public void OnClick_CloseMenu()
    {
        panel_inGame.SetActive(true);
        panel_Menu.SetActive(false);
    }

    public void SetCurrentLobbyName(string name)
    {
        txt_currentLobbyName.text = name;
    }

    public void OnClick_LeavePlayRoom()
    {
        Debug.Log("OnClick_LeavePlayroom();");
        ConnectionManager.instance.LeavePlayroom();
    }
}
