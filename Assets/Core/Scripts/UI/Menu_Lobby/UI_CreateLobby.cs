using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Playroom;
using static Util_UI;
using static NetworkingMessageAttributes;

public class UI_CreateLobby : MonoBehaviour
{
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

    private void Start()
    {
        SetUp();
    }

    public void SetUp()
    {
        string randomDefaultLobbyName = defaultLobbyNames[UnityEngine.Random.Range(0, defaultLobbyNames.Length - 1)];
        lobbyName_inputField.SetTextWithoutNotify(randomDefaultLobbyName);
        nameOfNewLobby = randomDefaultLobbyName;

        isPublic = true;
        lobbyPassword_inputField.gameObject.SetActive(false);
        password = "empty";

        map = Map.DefaultMap;

        maxPlayers = 4;
        playersInLobby_txt.text = maxPlayers.ToString();
        maxPlayersSlider.SetValueWithoutNotify(maxPlayers);
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

    public void OnClick_Cancel()
    {
        Destroy(gameObject);
    }

    public void OnClick_CreateLobby()
    {
        if (!IsStringClearFromErrors(nameOfNewLobby, errorText, Input_Field.Lobby_Name, 5, 20))
            return;

        if (!isPublic && !IsStringClearFromErrors(password, errorText, Input_Field.Password, 5, 15))
            return;

        // here everything is ok, we need to send the request to create lobby and then wait
        // "create_playroom|nameOfRoom|is_public|password|map|maxPlayers";
        ConnectionManager.instance.SendMessageToServer($"{CREATE_PLAY_ROOM}|{nameOfNewLobby}|{isPublic}|{password}|{map}|{maxPlayers}");
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
}
