using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NetworkingMessageAttributes;

public class UI_PlayroomsManager : MonoBehaviour
{
    public GameObject parentForLobbyItems;

    List<UI_PlayroomItem> lobbyItems;

    public void SpawnLobbyItems(string data)
    {
        ClearItemsHolder();

        List<Playroom> playrooms = CreatePlayroomsFromNetworkResponse(data);
        Debug.Log($"found {playrooms.Count} playrooms");

        lobbyItems = new List<UI_PlayroomItem>();

        foreach(Playroom a in playrooms)
        {
            GameObject gameObj = Instantiate(PrefabsHolder.instance.ui_lobbyItem_prefab);
            gameObj.transform.SetParent(parentForLobbyItems.transform);
            
            UI_PlayroomItem item = gameObj.GetComponent<UI_PlayroomItem>();
            item.SetUpValues(a);
            lobbyItems.Add(item);
        }
    }

    public void ClearItemsHolder()
    {
        List<GameObject> itemsToDelete = new List<GameObject>();
        for (int i = 0; i < parentForLobbyItems.transform.childCount; i++)
        {
            itemsToDelete.Add(parentForLobbyItems.transform.GetChild(i).gameObject);
        }
        foreach(GameObject a in itemsToDelete)
        {
            Destroy(a);
        }
    }

    // "playrooms_data_response|playroom_data(/),playroom_data, playroom_data"
    // data: id/nameOfRoom/is_public/password/map/currentPlayers/maxPlayers
    List<Playroom> CreatePlayroomsFromNetworkResponse(string networkResponse)
    {
        string[] sub1 = networkResponse.Split('|');
        string[] sub2 = sub1[1].Split(',');

        List<Playroom> playrooms = new List<Playroom>();

        foreach(string a in sub2)
        {
            Playroom room = new Playroom(a);
            playrooms.Add(room);
        }

        return playrooms;
    }
}
