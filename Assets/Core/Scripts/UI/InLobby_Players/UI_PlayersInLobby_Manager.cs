using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NetworkingMessageAttributes;

public class UI_PlayersInLobby_Manager : MonoBehaviour
{
    public GameObject parentForPlayerItems;

    List<UI_InLobby_PlayerDataItem> playersItems;

    // already cleaned up string, without '|' in the beginning
    public void SpawnLobbyItems(string data)
    {
        ClearItemsHolder();

        List<PlayerInLobbyData> playersData = CreatePlayersScoresFromNetworkResponse(data);
        //Debug.Log($"found {playersData.Count} player datas");

        playersItems = new List<UI_InLobby_PlayerDataItem>();

        foreach (PlayerInLobbyData a in playersData)
        {
            GameObject gameObj = Instantiate(PrefabsHolder.instance.ui_playerInLobbyItem_prefab, parentForPlayerItems.transform);
            //gameObj.transform.SetParent(parentForPlayerItems.transform);

            UI_InLobby_PlayerDataItem item = gameObj.GetComponent<UI_InLobby_PlayerDataItem>();
            item.SetUpData(a.nickname, a.killsCount, a.deathsCount);
            playersItems.Add(item);
        }
    }

    public void ClearItemsHolder()
    {
        List<GameObject> itemsToDelete = new List<GameObject>();
        for (int i = 0; i < parentForPlayerItems.transform.childCount; i++)
        {
            itemsToDelete.Add(parentForPlayerItems.transform.GetChild(i).gameObject);
        }
        foreach (GameObject a in itemsToDelete)
        {
            Destroy(a);
        }
    }

    // players_scores|data@data@data
    // {fullFataOfPlayersInThatRoom} => ip/nickname/kills/deaths@ip/nickname/kills/deaths@ip/nickname/kills/deaths
    List<PlayerInLobbyData> CreatePlayersScoresFromNetworkResponse(string networkResponse)
    {
        if (networkResponse.Equals(""))
        {
            Debug.LogError("parsing empty string!");
            return new List<PlayerInLobbyData>();
        }
        
        List<PlayerInLobbyData> _playersDataInLobby = new List<PlayerInLobbyData>();

        if (networkResponse.Contains("@"))
        {
            string[] sub = networkResponse.Split('@');
            foreach (string a in sub)
            {
                PlayerInLobbyData playerData = new PlayerInLobbyData(a);
                _playersDataInLobby.Add(playerData);
            }
        }
        else
        {
            PlayerInLobbyData playerData = new PlayerInLobbyData(networkResponse);
            _playersDataInLobby.Add(playerData);
        }

        return _playersDataInLobby;
    }

    public class PlayerInLobbyData
    {
        public string ip;
        public string nickname;
        public int killsCount;
        public int deathsCount;


        public PlayerInLobbyData(string createFrom)
        {
            try
            {
                string[] substrings = createFrom.Split('/');
                ip = substrings[0];
                nickname = substrings[1];
                killsCount = Int32.Parse(substrings[2]);
                deathsCount = Int32.Parse(substrings[3]);

            } catch (Exception e) { Debug.Log(e.ToString()); }

        }

        public PlayerInLobbyData(string _ip, string _nickname, int _killsCount, int _deathsCount)
        {
            ip = _ip;
            nickname = _nickname;
            killsCount = _killsCount;
            deathsCount = _deathsCount;
        }
    }
}
