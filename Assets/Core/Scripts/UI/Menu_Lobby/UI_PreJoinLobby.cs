using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static NetworkingMessageAttributes;
using static Util_UI;
using static DataTypes;
using static EnumsAndData;

public class UI_PreJoinLobby : MonoBehaviour
{
    Playroom assignedPlayroom;
    string passwordEntered;

    public TMP_Text error_response;

    public void SetUp(Playroom playroom)
    {
        assignedPlayroom = playroom;
        if (assignedPlayroom.isPublic)
        {
            passwordedPanel.SetActive(false);
            freeToJoinPanel.SetActive(true);
        }
        else
        {
            passwordedPanel.SetActive(true);
            freeToJoinPanel.SetActive(false);
        }
    }

    public void OnEditText_EnterPassword(string pass)
    {
        passwordEntered = pass;
    }

    public GameObject passwordedPanel;
    public GameObject freeToJoinPanel;
    // "enter_playroom|3251|the_greatest_password_ever";
    public void OnClick_JoinLobby()
    {
        if (assignedPlayroom.isPublic)
        {
            ConnectionManager.instance.SendMessageToServer($"{ENTER_PLAY_ROOM}|{assignedPlayroom.id}");
        }
        else
        {
            if (!IsStringClearFromErrors(passwordEntered, error_response, Input_Field.Password, 5, 15))
                return;

            ConnectionManager.instance.SendMessageToServer($"{ENTER_PLAY_ROOM}|{assignedPlayroom.id}|{passwordEntered}");
        }

        Destroy(gameObject);
    }

    public void OnClick_Cancel()
    {
        Destroy(gameObject);
    }
}
