using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_PlayroomItem : MonoBehaviour
{
    public TMP_Text txtLobbyName;
    public TMP_Text txtLobbyType;
    public TMP_Text txtLobbyMap;

    public TMP_Text txtLobbyPlayers;

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
    }

    public void OnClick_LobbyItem()
    {
        GameObject newObj = Instantiate(PrefabsHolder.instance.ui_preJoinLobby_prefab, UI_GlobalManager.instance.UI_mainMenu.rootForServerMessages.transform);
        UI_PreJoinLobby uiPreJoin = newObj.GetComponent<UI_PreJoinLobby>();
        uiPreJoin.SetUp(playroom);
    }
}
