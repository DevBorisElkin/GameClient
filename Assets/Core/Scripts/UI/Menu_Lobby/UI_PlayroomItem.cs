using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DataTypes;
using static EnumsAndData;

public class UI_PlayroomItem : MonoBehaviour
{
    public TMP_Text txtLobbyName;
    public TMP_Text txtLobbyType;
    public TMP_Text txtLobbyMap;

    public TMP_Text txtLobbyPlayers;
    public TMP_Text txtLobbyStatus;
    public TMP_Text txtTimeLeftForMatch;

    public Playroom playroom;

    public void SetUpValues(Playroom _playroom)
    {
        playroom = _playroom;
        txtLobbyName.text = playroom.name;

        if (playroom.isPublic)
            txtLobbyType.text = "Public";
        else txtLobbyType.text = "Private";
        txtLobbyMap.text = playroom.map.ToString();
        txtLobbyPlayers.text = $"{playroom.playersCurrAmount} / {playroom.maxPlayers}";

        txtLobbyStatus.text = $"{playroom.matchState}";
        txtTimeLeftForMatch.text = $"{UI_InGame.ConvertTimeSecondsIntoMinsSecs(playroom.totalTimeToFinishInSeconds.Value)}";
    }

    public void OnClick_LobbyItem()
    {
        GameObject newObj = Instantiate(PrefabsHolder.instance.ui_preJoinLobby_prefab, UI_GlobalManager.instance.UI_mainMenu.rootForServerMessages.transform);
        UI_PreJoinLobby uiPreJoin = newObj.GetComponent<UI_PreJoinLobby>();
        uiPreJoin.SetUp(playroom);
    }
}
