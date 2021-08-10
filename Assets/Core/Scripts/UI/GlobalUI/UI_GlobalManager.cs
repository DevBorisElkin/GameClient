using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ConnectionManager;
using static Enums;

public class UI_GlobalManager : MonoBehaviour
{
    public static UI_GlobalManager instance;

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

    UI_MainMenu _ui_MainMenu;
    public UI_MainMenu UI_mainMenu
    {
        get
        {
            if (_ui_MainMenu == null)
            {
                _ui_MainMenu = FindObjectOfType<UI_MainMenu>();
                return _ui_MainMenu;
            }
            else return _ui_MainMenu;
        }
    }

    public ClientStatus recordedStatus = ClientStatus.Disconnected;
    
    // is called ONLY when new scene loaded (at Start() methods of UI_1 & UI_2)
    public void ManageSceneOnLoad()
    {
        Debug.Log("Manage Scene On Load()");
        if(SceneManager.GetActiveScene().buildIndex == 0) // Intro Panel
        {
            // nothing here for now
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1) // Main Menu Panel
        {
            UI_mainMenu.MainMenu_Connected(recordedStatus, true);
        }
    }
    // is called manually
    public void ManageScene(ClientStatus newStatus)
    {
        //Debug.Log("Manage Scene()");
        if (recordedStatus.Equals(ClientStatus.Disconnected) && newStatus.Equals(ClientStatus.Connected))
        {
            Action act = updateUIInMainMenu;
            UnityThread.executeInUpdate(act);

            void updateUIInMainMenu() { if(UI_mainMenu!=null) UI_mainMenu.MainMenu_Connected(ClientStatus.Connected, false); }
        }
        else if (recordedStatus.Equals(ClientStatus.Connected) && newStatus.Equals(ClientStatus.Disconnected))
        {
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene() { UI_mainMenu.MainMenu_Connected(ClientStatus.Disconnected, false); }
        }
        else if (recordedStatus.Equals(ClientStatus.Connected) && newStatus.Equals(ClientStatus.Authenticated))
        {
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene() { UI_mainMenu.MainMenu_Connected(ClientStatus.Authenticated, true); }
        }
        else if (recordedStatus.Equals(ClientStatus.Authenticated) && newStatus.Equals(ClientStatus.Connected))
        {
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene() { UI_mainMenu.MainMenu_Connected(ClientStatus.Connected, true); }
        }
        else if (recordedStatus.Equals(ClientStatus.Authenticated) && newStatus.Equals(ClientStatus.Disconnected))
        {
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene() { UI_mainMenu.MainMenu_Connected(ClientStatus.Disconnected, true); }
        }
        else if (recordedStatus.Equals(ClientStatus.Authenticated) && newStatus.Equals(ClientStatus.InPlayRoom))
        {
            // Load play room scene
            Action act = LoadPlayRoomScene;
            UnityThread.executeInUpdate(act);

            void LoadPlayRoomScene()
            {
                SceneManager.LoadSceneAsync("NetworkingGameScene");
            }
        }
        else if (recordedStatus.Equals(ClientStatus.InPlayRoom) && ((newStatus.Equals(ClientStatus.Authenticated) || newStatus.Equals(ClientStatus.Disconnected) || newStatus.Equals(ClientStatus.Connected))))
        {
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene()
            {
                Debug.Log("Leaving playroom");
                SceneManager.LoadSceneAsync("MainScene");
            }

        }
        Debug.Log($"Manage Scene(); Old status [{recordedStatus}], New status [{newStatus}]");
        recordedStatus = newStatus;
    }
    public void SetAuthInResult(RequestResult result)
    {
        if(recordedStatus.Equals(ClientStatus.Connected) || recordedStatus.Equals(ClientStatus.Disconnected))
        {
            UI_mainMenu.ShowAuthOrRegResult(Util_UI.InternetRequestResultToString(result), true);
        }
    }
    public void SetRegistrationResult(RequestResult result)
    {
        if (recordedStatus.Equals(ClientStatus.Connected) || recordedStatus.Equals(ClientStatus.Disconnected))
        {
            UI_mainMenu.ShowAuthOrRegResult(Util_UI.InternetRequestResultToString(result), false);
        }
    }

    public void UpdatePlyroomsList(string msg)
    {
        Action act = Action; ;
        UnityThread.executeInUpdate(act);

        void Action()
        {
            UI_mainMenu.ui_PlayroomsManager.SpawnLobbyItems(msg);
        }
    }
    public void ShowLatestMessageFromServer(string msg)
    {
        Action act = Act;
        UnityThread.executeInUpdate(act);

        void Act()
        {
            GameObject spawnedGobj = Instantiate(PrefabsHolder.instance.ui_messageFromServer_prefab, UI_mainMenu.rootForServerMessages.transform);
            UI_MessageFromServer message = spawnedGobj.GetComponent<UI_MessageFromServer>();
            message.SetUp(msg);
        }
    }


}
