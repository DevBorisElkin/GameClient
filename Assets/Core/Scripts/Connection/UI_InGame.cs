using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EnumsAndData;
using static NetworkingMessageAttributes;

public class UI_InGame : MonoBehaviour
{
    public static UI_InGame instance;

    public GameObject panel_inGame;
    public GameObject panel_Menu;

    public TMP_Text txt_currentLobbyName;

    [Header("Playroom details and waiting state")]
    public TMP_Text waitingForPlayersTxt;

    public GameObject timeLeftPanel;
    public TMP_Text timeLeftTxt;

    public GameObject killsToFinishPanel;
    public TMP_Text killsToFinishTxt;

    public GameObject matchFinishedPanel;
    public TMP_Text txtMatchResult;
    public TMP_Text txtMatchWinner;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
    }

    private void Start()
    {
        OnClick_CloseMenu();
        OnlineGameManager.instance.ui_PlayersInLobby_Manager.SpawnLobbyItems(OnlineGameManager.currentPlayersScores_OnEnter);
        txt_currentLobbyName.text = OnlineGameManager.currentLobbyName_OnEnter;

        //StartCoroutine(DelayedUpdateMatchStateUI(ConnectionManager.activePlayroom.matchState));
        OnNewMatchState(ConnectionManager.activePlayroom.matchState);
    }
    IEnumerator DelayedUpdateMatchStateUI(MatchState newState)
    {
        yield return new WaitForSeconds(0.1f);
        OnNewMatchState(newState);
    }

    public void OnNewMatchState(MatchState newState)
    {
        if(newState == MatchState.WaitingForPlayers)
        {
            waitingForPlayersTxt.gameObject.SetActive(true);
            waitingForPlayersTxt.text = waitingForPlayersString + $"{(ConnectionManager.activePlayroom.playersToStart - OnlineGameManager.instance.opponents.Count - 1)}";
            timeLeftPanel.SetActive(false);
            killsToFinishPanel.SetActive(false);
        }
        else if(newState == MatchState.InGame)
        {
            waitingForPlayersTxt.gameObject.SetActive(false);
            timeLeftPanel.SetActive(true);
            killsToFinishPanel.SetActive(true);
            timeLeftTxt.text = ConvertTimeSecondsIntoMinsSecs(ConnectionManager.activePlayroom.totalTimeToFinishInSeconds);
            killsToFinishTxt.text = ConnectionManager.activePlayroom.killsToFinish.ToString();
        }
        else if(newState == MatchState.Finished)
        {
            waitingForPlayersTxt.gameObject.SetActive(false);
            timeLeftPanel.SetActive(false);
            killsToFinishPanel.SetActive(false);
        }
    }

    public void UpdateWaitingForPlayersText(int opponentsCount)
    {
        waitingForPlayersTxt.text = waitingForPlayersString + $"{(ConnectionManager.activePlayroom.playersToStart - opponentsCount - 1)}";
    }
    public void UpdateTimeLeftTxt(int newSeconds)
    {
        timeLeftTxt.text = ConvertTimeSecondsIntoMinsSecs(newSeconds);
    }

    public void OnMatchResult(MatchResult result)
    {
        OnNewMatchState(MatchState.Finished);
        matchFinishedPanel.SetActive(true);
        txtMatchResult.text = MatchResultToString(result);
        txtMatchWinner.text = ConnectionManager.activePlayroom.winnerNickname;
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


    // Addons

    static string waitingForPlayersString = "Waiting for players to start...\nPlayers to join left: ";

    static string ConvertTimeSecondsIntoMinsSecs(int timeInSeconds)
    {
        int totalMins = (int)TimeSpan.FromSeconds(timeInSeconds).TotalMinutes;
        int totalSecs = timeInSeconds - (totalMins * 60);
        if (totalMins > 0)
            return $"{totalMins}:{totalSecs}";
        else return timeInSeconds.ToString();
    }

    static string MatchResultToString(MatchResult res)
    {
        if(res == MatchResult.PlayerWon)
        {
            return "Congractulations! Player WON";
        }else if (res == MatchResult.Draw)
        {
            return "No one won the match, it's a Draw";
        } else if (res == MatchResult.Discarded)
        {
            return "Match was discarded";
        }
        return "";
    }

    public void OnClick_ToMainMenu()
    {
        ConnectionManager.instance.LeavePlayroom();
    }
}
