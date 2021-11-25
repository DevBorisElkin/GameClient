using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EnumsAndData;
using static NetworkingMessageAttributes;
using UniRx;

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

        ManageSubscriptions(true);
    }

    public void OnNewMatchState(MatchState newState)
    {
        if(newState == MatchState.WaitingForPlayers)
        {
            waitingForPlayersTxt.gameObject.SetActive(true);
            waitingForPlayersTxt.text = waitingForPlayersString + $"{(ConnectionManager.activePlayroom.playersToStart - OnlineGameManager.instance.opponents.Count - 1)}";
            timeLeftPanel.SetActive(false);
            killsToFinishPanel.SetActive(false);
            //MatchStartCountdown(false);
        }
        else if(newState == MatchState.InGame)
        {
            waitingForPlayersTxt.gameObject.SetActive(false);
            timeLeftPanel.SetActive(true);
            killsToFinishPanel.SetActive(true);
            timeLeftTxt.text = ConvertTimeSecondsIntoMinsSecs(ConnectionManager.activePlayroom.totalTimeToFinishInSeconds.Value);
            killsToFinishTxt.text = ConnectionManager.activePlayroom.killsToFinish.ToString();
            //MatchStartCountdown(false);
        }
        else if(newState == MatchState.Finished)
        {
            waitingForPlayersTxt.gameObject.SetActive(false);
            timeLeftPanel.SetActive(false);
            killsToFinishPanel.SetActive(false);
            //MatchStartCountdown(false);
        }
        else if(newState == MatchState.JustStarting)
        {
            waitingForPlayersTxt.gameObject.SetActive(false);
            //MatchStartCountdown(true);
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
        if (!pl.dbIdOflastHitPlayer.Equals("") && EventManager.isAlive)
            ConnectionManager.instance.SendMessageToServer($"{PLAYER_DIED}|{pl.dbIdOflastHitPlayer}|{DeathDetails.FellOutOfMap}");

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

    #region Match Start Countdown

    [Header("Match start countdown")]
    public GameObject matchStartCountdownPanel;
    public TMP_Text basicCountdownTmpText;
    public TMP_Text timeLeftTmpText;
    public string basicCountdownText = "Match Starts In";

    int startMatchCountdown = 8;
    //public void MatchStartCountdown(bool state)
    //{
    //    Debug.Log($"MatchStartCountdown {state}");
    //    if (state)
    //    {
    //        int tmp = ConnectionManager.activePlayroom.totalTimeToFinishInSecUnchanged - ConnectionManager.activePlayroom.totalTimeToFinishInSeconds.Value;
    //        int secondsTillStart = startMatchCountdown - tmp;
    //        Debug.Log($"ConnectionManager.activePlayroom.totalTimeToFinishInSecUnchanged [{ConnectionManager.activePlayroom.totalTimeToFinishInSecUnchanged}] - " +
    //            $"ConnectionManager.activePlayroom.totalTimeToFinishInSeconds[{ConnectionManager.activePlayroom.totalTimeToFinishInSeconds}], " +
    //            $"secondsTillStart: [{secondsTillStart}]");
    //        if (secondsTillStart > 8) return;
    //        StartCoroutine(MatchStartCountdown(secondsTillStart));
    //    }
    //    else
    //    {
    //        basicCountdownTmpText.gameObject.SetActive(false);
    //        timeLeftTmpText.gameObject.SetActive(false);
    //        matchStartCountdownPanel.SetActive(false);
    //    }
    //}


    List<System.IDisposable> LifetimeDisposables;
    void ManageSubscriptions(bool subscribe)
    {
        if (subscribe)
        {
            LifetimeDisposables = new List<IDisposable>();
            ConnectionManager.activePlayroom.totalTimeToFinishInSeconds.Subscribe(_ => { 
                UpdateTimeLeftTxt(_);
                ManageMatchStart(_);
            }).AddTo(LifetimeDisposables);
            ConnectionManager.activePlayroom.matchState.Subscribe(_ => {
                OnNewMatchState(ConnectionManager.activePlayroom.matchState.Value);
            }).AddTo(LifetimeDisposables);
        }
        else
        {
            if (LifetimeDisposables != null && LifetimeDisposables.Count > 0)
                foreach (var a in LifetimeDisposables)
                    a.Dispose();
        }
    }

    void ManageMatchStart(int secondsLeft)
    {
        if (ConnectionManager.activePlayroom.matchState.Value != MatchState.JustStarting) return;

        int tmp = ConnectionManager.activePlayroom.totalTimeToFinishInSecUnchanged - secondsLeft;
        int secondsTillStart = startMatchCountdown - tmp;

        basicCountdownTmpText.text = basicCountdownText;
        matchStartCountdownPanel.SetActive(true);

        if (secondsTillStart > 5)
        {
            basicCountdownTmpText.gameObject.SetActive(true);
            timeLeftTmpText.gameObject.SetActive(false);
        }
        else if(secondsTillStart > 0)
        {
            basicCountdownTmpText.gameObject.SetActive(false);
            timeLeftTmpText.gameObject.SetActive(true);

            timeLeftTmpText.text = secondsTillStart.ToString();
        }else if(secondsTillStart <= 0)
        {
            matchStartCountdownPanel.SetActive(false);
            ConnectionManager.activePlayroom.matchState.Value = MatchState.InGame;
        }
    }

    private void OnDestroy()
    {
        ManageSubscriptions(false);
    }

    #endregion

}
