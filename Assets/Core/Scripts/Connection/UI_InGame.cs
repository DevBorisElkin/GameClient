using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EnumsAndData;
using static NetworkingMessageAttributes;
using UniRx;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.UI;

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

    [Header("TweensSettings")]
    public CanvasGroup panel_inGame_CanvasGroup;
    public RectTransform panelMenuRectTransform;
    public Image panelMenuBackround;
    public Color panelMenuBackgroundNormalColor;
    public float panelInGame_OpenCloseTime = 0.3f;

    [Header("MatchFinishedPanel")]
    public WinScreen winScreen;

    [Header("Skybox Rotation")]
    public float skyboxRotationSpeed = 1f;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
    }

    private void Start()
    {
        RenderSettings.skybox.SetFloat("_Rotation", 0f);
        OnClick_CloseMenu();
        OnlineGameManager.instance.ui_PlayersInLobby_Manager.SpawnLobbyItems(OnlineGameManager.currentPlayersScores_OnEnter);
        txt_currentLobbyName.text = OnlineGameManager.currentLobbyName_OnEnter;

        ManageSubscriptions(true);
    }

    private void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyboxRotationSpeed);
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
            timeLeftPanel.SetActive(false);
            killsToFinishPanel.SetActive(false);
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

    public void OnClick_OpenMenu()
    {
        panel_Menu.SetActive(true);

        panel_inGame_CanvasGroup.alpha = 1f;
        panelMenuRectTransform.anchoredPosition = new Vector2(-panelMenuRectTransform.rect.width, 0);
        panelMenuBackround.color = Color.clear;

        panel_inGame_CanvasGroup.DOFade(0f, panelInGame_OpenCloseTime);
        DOTween.To(() => panelMenuRectTransform.anchoredPosition, x => panelMenuRectTransform.anchoredPosition = x, new Vector2(0f, 0f), panelInGame_OpenCloseTime);
        panelMenuBackround.DOColor(panelMenuBackgroundNormalColor, panelInGame_OpenCloseTime);
    }
    public void OnClick_CloseMenu()
    {
        panel_inGame_CanvasGroup.alpha = 0f;
        panelMenuRectTransform.anchoredPosition = new Vector2(0, 0);
        panelMenuBackround.color = panelMenuBackgroundNormalColor;

        panel_inGame_CanvasGroup.DOFade(1f, panelInGame_OpenCloseTime);
        DOTween.To(() => panelMenuRectTransform.anchoredPosition, x => panelMenuRectTransform.anchoredPosition = x, new Vector2(-panelMenuRectTransform.rect.width, 0f), panelInGame_OpenCloseTime);
        panelMenuBackround.DOColor(Color.clear, panelInGame_OpenCloseTime).OnComplete(() => {
            panel_Menu.SetActive(false);
        });
    }

    public void AdminPanelOpened(bool state)
    {
        if (state)
        {
            panel_inGame_CanvasGroup.alpha = 1f;
            panel_inGame_CanvasGroup.DOFade(0f, panelInGame_OpenCloseTime);
        }
        else
        {
            panel_inGame_CanvasGroup.alpha = 0f;
            panel_inGame_CanvasGroup.DOFade(1f, panelInGame_OpenCloseTime);
        }


        panel_inGame.SetActive(!state);
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

    public static string ConvertTimeSecondsIntoMinsSecs(int timeInSeconds)
    {
        int totalMins = (int)TimeSpan.FromSeconds(timeInSeconds).TotalMinutes;
        int totalSecs = timeInSeconds - (totalMins * 60);
        if (totalMins > 0)
            return $"{totalMins}:{totalSecs}";
        else return timeInSeconds.ToString();
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
    public string matchStartedText = "Match Started";
    public float scaleInAnimTime = 0.25f;
    public float basicTextFadeOutDelay = 1f;
    public float basicTextFadeOutTime = 0.35f;
    Color transparentColor = new Color(0, 0, 0, 0);

    int startMatchCountdown = 8;

    Tween matchStartsInAnimation;
    bool firstCallForBacisStartText;

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

    async void ManageMatchStart(int secondsLeft)
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
            await AnimateTextScale(basicCountdownTmpText.transform);
        }
        else if(secondsTillStart > 0)
        {
            VibrationsManager.OnMatchStarting_Vibrations();

            basicCountdownTmpText.gameObject.SetActive(false);
            timeLeftTmpText.gameObject.SetActive(true);

            timeLeftTmpText.text = secondsTillStart.ToString();
            await AnimateTextScale(timeLeftTmpText.transform);
        }
        else if(secondsTillStart == 0)
        {
            VibrationsManager.OnMatchStarted_Vibrations();

            ConnectionManager.activePlayroom.matchState.Value = MatchState.InGame;

            basicCountdownTmpText.gameObject.SetActive(true);
            timeLeftTmpText.gameObject.SetActive(false);

            basicCountdownTmpText.text = matchStartedText;
            firstCallForBacisStartText = false;
            await AnimateTextScale(basicCountdownTmpText.transform);
            AnimateMatchStartEndText(basicCountdownTmpText);
        }
    }
    async Task AnimateTextScale(Transform gameObjectTr)
    {
        if(basicCountdownTmpText.transform == gameObjectTr)
        {
            if (firstCallForBacisStartText) return;
            firstCallForBacisStartText = true;
        }
        ResetMatchStartAnimationTween();
        gameObjectTr.localScale = Vector3.zero;
        matchStartsInAnimation = gameObjectTr.DOScale(Vector3.one, scaleInAnimTime);
        await matchStartsInAnimation.AsyncWaitForCompletion();
    }

    void AnimateMatchStartEndText(TMP_Text matchStartedText)
    {
        Observable.Timer(TimeSpan.FromSeconds(basicTextFadeOutDelay)).Subscribe(_ => {
            matchStartedText.DOColor(transparentColor, basicTextFadeOutTime).OnComplete(() => {
                matchStartCountdownPanel.SetActive(false);
            });
        }).AddTo(LifetimeDisposables);
    }

    void ResetMatchStartAnimationTween()
    {
        if (matchStartsInAnimation != null)
        {
            matchStartsInAnimation.Complete();
            matchStartsInAnimation = null;
        }
    }

    private void OnDestroy()
    {
        RenderSettings.skybox.SetFloat("_Rotation", 0f);
        ManageSubscriptions(false);
    }

    #endregion

}
