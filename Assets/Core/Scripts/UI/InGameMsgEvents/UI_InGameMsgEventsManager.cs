using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumsAndData;
using static OnlineGameManager;

public class UI_InGameMsgEventsManager : MonoBehaviour
{
    #region Singleton
    public static UI_InGameMsgEventsManager instance;

    private void Awake()
    {
        InitSingleton();
    }
    void InitSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
    }
    #endregion

    public Transform parentForEventMessages;

    // player_was_killed_message|playerDeadNickname/playerDeadIP|playerKillerNickname/playerKilledIP|deathDetails
    // returns db_id of dead player
    public int FromServer_DeathEventMessageReceived(string message)
    {
        string[] substring1 = message.Split('|');
        string[] playerDeadSubstring = substring1[1].Split('/');
        string[] killerSubstring = substring1[2].Split('/');
        string deathDetailsStr = substring1[3];
        Enum.TryParse<DeathDetails>(deathDetailsStr, out DeathDetails deathDetailsResult);

        MessageType messageType;
        ReasonOfDeath reasonOfDeath;
        if (killerSubstring[0].Equals("none"))
        {
            messageType = MessageType.Suicide;
            reasonOfDeath = ReasonOfDeath.Suicide;
        }
        else
        {
            messageType = MessageType.Kill;
            reasonOfDeath = ReasonOfDeath.ByOtherPlayer;
        }

        GameObject msgEventPanel = Instantiate(PrefabsHolder.instance.ui_inGameEventMessage, parentForEventMessages);
        UI_InGameEventMessageItem item = msgEventPanel.GetComponent<UI_InGameEventMessageItem>();
        item.SetUpDeathMessage(messageType, deathDetailsResult, reasonOfDeath, killerSubstring[0], playerDeadSubstring[0]);
        return Int32.Parse(playerDeadSubstring[1]);
    }
    public void FromServer_PlayerJoinedPlayroomMessageReceived(string newUserNickname)
    {
        GameObject msgEventPanel = Instantiate(PrefabsHolder.instance.ui_inGameEventMessage, parentForEventMessages);
        UI_InGameEventMessageItem item = msgEventPanel.GetComponent<UI_InGameEventMessageItem>();
        item.OnEnterExitMessage(MessageType.Enter, newUserNickname);
    }
    public void FromServer_PlayerExitedPlayroomMessageReceived(string userNickname)
    {
        GameObject msgEventPanel = Instantiate(PrefabsHolder.instance.ui_inGameEventMessage, parentForEventMessages);
        UI_InGameEventMessageItem item = msgEventPanel.GetComponent<UI_InGameEventMessageItem>();
        item.OnEnterExitMessage(MessageType.Exit, userNickname);
    }

    public void FromServer_PlayerPickedUpRune(Rune runeType, string nicknameOfPicker)
    {
        GameObject msgEventPanel = Instantiate(PrefabsHolder.instance.ui_inGameEventMessage, parentForEventMessages);
        UI_InGameEventMessageItem item = msgEventPanel.GetComponent<UI_InGameEventMessageItem>();
        item.OnRunePickupMessage(runeType, nicknameOfPicker);
    }

    public void FromServer_RuneSpawned(Rune runeType, PlayerData runeInvoker)
    {
        GameObject msgEventPanel = Instantiate(PrefabsHolder.instance.ui_inGameEventMessage, parentForEventMessages);
        UI_InGameEventMessageItem item = msgEventPanel.GetComponent<UI_InGameEventMessageItem>();
        item.OnRuneSpawnedMessage(runeType, runeInvoker);
    }
}
