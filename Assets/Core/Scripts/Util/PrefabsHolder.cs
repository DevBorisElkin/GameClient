using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabsHolder : MonoBehaviour
{
    public static PrefabsHolder instance;
    private void Awake()
    {
        InitSingleton();
    }

    void InitSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(gameObject);
    }


    public GameObject player_prefab;
    public GameObject opponent_prefab;

    public GameObject ui_preJoinLobby_prefab;
    public GameObject ui_lobbyItem_prefab;
    public GameObject ui_createLobby_prefab;
    public GameObject ui_messageFromServer_prefab;
}
