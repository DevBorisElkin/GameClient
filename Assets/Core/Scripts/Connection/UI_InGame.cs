using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static EnumsAndData;
using static NetworkingMessageAttributes;

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
        PlayerMovementController pl = FindObjectOfType<PlayerMovementController>();
        // ensure that players won't abuse leaving from playroom to avoid giving opponents score points
        if (!pl.ipOfLastHitPlayer.Equals("") && EventManager.isAlive)
            ConnectionManager.instance.SendMessageToServer($"{PLAYER_DIED}|{pl.ipOfLastHitPlayer}|{DeathDetails.FellOutOfMap}");

        Debug.Log("OnClick_LeavePlayroom();");
        ConnectionManager.instance.LeavePlayroom();
    }
}
