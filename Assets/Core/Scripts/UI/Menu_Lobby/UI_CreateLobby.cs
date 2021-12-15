using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DataTypes;
using static EnumsAndData;
using static Util_UI;
using static NetworkingMessageAttributes;
using DG.Tweening;
using UniRx;
using System;

public class UI_CreateLobby : MonoBehaviour
{
    [Header("MainSettings")]
    public TMP_InputField lobbyName_inputField;

    public Toggle isPublic_toggle;
    public TMP_InputField lobbyPassword_inputField;

    public TMP_Dropdown mapChoice_Dropdown;

    public TMP_Text playersInLobby_txt;
    public Slider maxPlayersSlider;

    [Space(4f)]
    public TMP_Text errorText;

    string nameOfNewLobby;
    bool isPublic;
    string password;
    Map map;
    int maxPlayers;

    [Header("AdditionalSettings")]
    public GameObject additionalSettingsPanel;

    public Slider playersToStartMatchSlider;
    public Slider killsToFinishMatchSlider;
    public Slider timeOfMatchSlider;

    public TMP_Text playersToStartMatchTxt;
    public TMP_Text killsToFinishMatchTxt;
    public TMP_Text timeOfMatchTxt;

    int playersToStartMatch;
    int killsToFinishMatch;
    int timeOfMatch;
    

    private void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        canInteractWithButtons = true;
        normalBackgroundColor = backgroundImage.color;

        string randomDefaultLobbyName = defaultLobbyNames[UnityEngine.Random.Range(0, defaultLobbyNames.Length - 1)];
        lobbyName_inputField.SetTextWithoutNotify(randomDefaultLobbyName);
        nameOfNewLobby = randomDefaultLobbyName;

        isPublic = true;
        lobbyPassword_inputField.gameObject.SetActive(false);
        password = "";

        map = Map.DefaultMap;

        maxPlayers = 4;
        playersInLobby_txt.text = maxPlayers.ToString();
        maxPlayersSlider.SetValueWithoutNotify(maxPlayers);

        // additional settings
        playersToStartMatch = 2;
        playersToStartMatchTxt.text = playersToStartMatch.ToString();
        playersToStartMatchSlider.SetValueWithoutNotify(playersToStartMatch);

        killsToFinishMatch = 10;
        killsToFinishMatchTxt.text = killsToFinishMatch.ToString();
        killsToFinishMatchSlider.SetValueWithoutNotify(killsToFinishMatch);

        timeOfMatch = 15;
        timeOfMatchTxt.text = timeOfMatch.ToString();
        timeOfMatchSlider.SetValueWithoutNotify(timeOfMatch);

        SetMainPanelAnimationState(true);
    }

    public void OnEditInputField_SetLobbyName(string text)
    {
        nameOfNewLobby = text;
    }
    public void OnChangeToggle_ChangeLobbyPrivacy(bool val)
    {
        isPublic = val;
        lobbyPassword_inputField.gameObject.SetActive(!val);
    }
    public void OnEditInputField_SetLobbyPassword(string text)
    {
        password = text;
    }
    public void OnDropdown_MapChoice(int val)
    {
        map = (Map)val;
    }

    public void OnSlider_MaxPlayersChanged(float val)
    {
        maxPlayers = (int)val;
        playersInLobby_txt.text = maxPlayers.ToString();
    }

    // Additional settings
    public void OnClick_OpenAdditionalSettings()
    {
        SetAdditionalSettingsAnimationState(true);
    }
    public void OnClick_CloseAdditionalSettings()
    {
        SetAdditionalSettingsAnimationState(false);
    }
    
    public void OnSlider_PlayersToStartChanged(float val)
    {
        playersToStartMatch = (int)val;
        playersToStartMatchTxt.text = playersToStartMatch.ToString();
    }
    public void OnSlider_KillsToFinishChanged(float val)
    {
        killsToFinishMatch = (int)val;
        killsToFinishMatchTxt.text = killsToFinishMatch.ToString();
    }
    public void OnSlider_TimeOfMatchChanged(float val)
    {
        timeOfMatch = (int)val;
        timeOfMatchTxt.text = timeOfMatch.ToString();
    }

    public void OnClick_Cancel()
    {
        SetMainPanelAnimationState(false);
    }

    public void OnClick_CreateLobby()
    {
        if (!canInteractWithButtons) return;
        canInteractWithButtons = false;
        Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => { canInteractWithButtons = true; });

        Debug.Log($"OnClick_CreateLobby {nameOfNewLobby} {password}");

        if (!IsStringClearFromErrors(nameOfNewLobby, null, Input_Field.Lobby_Name, 5, 20))
            return;

        if (!isPublic && !IsStringClearFromErrors(password, null, Input_Field.Password, 5, 15))
            return;

        // here everything is ok, we need to send the request to create lobby and then wait
        ConnectionManager.instance.SendMessageToServer($"{CREATE_PLAY_ROOM}|{nameOfNewLobby}|{isPublic}|{password}|{map}|{maxPlayers}|" +
            $"{playersToStartMatch}|{killsToFinishMatch}|{timeOfMatch}");
    }



    static string[] defaultLobbyNames = new string[10]
    {
        "Awesome_playroom",
        "Cool_lobby",
        "Clash_of_bros",
        "Lets_kick_it",
        "Join_all",
        "Cruel_massacre",
        "Bloody_massacre",
        "Easy_game",
        "Cool_fight",
        "Join_me_right_now"
    };


    [Header("Tween Settings")]
    public Image backgroundImage;
    Color normalBackgroundColor;
    public GameObject modalWindowParent;
    public CanvasGroup additionalSettingsCG;

    [Space(5f)]
    public float mainWindowOpenCloseTime = 0.3f;
    public float addSettingsOpenCloseTime = 0.3f;

    public bool canInteractWithButtons = true;

    public void SetMainPanelAnimationState(bool state)
    {
        if (!canInteractWithButtons) return;
        canInteractWithButtons = false;
        Observable.Timer(TimeSpan.FromSeconds(mainWindowOpenCloseTime)).Subscribe(_ => { canInteractWithButtons = true; });

        if (state)
        {
            backgroundImage.color = new Color(normalBackgroundColor.r, normalBackgroundColor.g, normalBackgroundColor.b, 0f);
            modalWindowParent.transform.localScale = Vector3.zero;

            backgroundImage.DOColor(normalBackgroundColor, mainWindowOpenCloseTime);
            modalWindowParent.transform.DOScale(Vector3.one, mainWindowOpenCloseTime);
        }
        else
        {
            backgroundImage.color = normalBackgroundColor;
            modalWindowParent.transform.localScale = Vector3.one;

            backgroundImage.DOColor(new Color(normalBackgroundColor.r, normalBackgroundColor.g, normalBackgroundColor.b, 0f), mainWindowOpenCloseTime);
            modalWindowParent.transform.DOScale(Vector3.zero, mainWindowOpenCloseTime).OnComplete(() => { Destroy(gameObject); });
        }
    }

    public void SetAdditionalSettingsAnimationState(bool state)
    {
        if (!canInteractWithButtons) return;
        canInteractWithButtons = false;
        Observable.Timer(TimeSpan.FromSeconds(addSettingsOpenCloseTime)).Subscribe(_ => { canInteractWithButtons = true; });

        if (state)
        {
            additionalSettingsCG.gameObject.SetActive(true);
            additionalSettingsCG.alpha = 0;
            additionalSettingsCG.DOFade(1f, addSettingsOpenCloseTime);
        }
        else
        {
            additionalSettingsCG.alpha = 1;
            additionalSettingsCG.DOFade(0f, addSettingsOpenCloseTime).OnComplete(() => {
                additionalSettingsCG.gameObject.SetActive(false);
            });
        }
    }
}
