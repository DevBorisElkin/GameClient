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
        // TODO OPEN SPEC PANEL TO JOIN WITH PASS OR FREE
    }
}
