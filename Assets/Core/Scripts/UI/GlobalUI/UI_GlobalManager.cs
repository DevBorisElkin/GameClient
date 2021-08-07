using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ConnectionManager;

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
        else
            if (instance != this) Destroy(gameObject);
    }

    UI_MainMenu _ui_MainMenu;
    public UI_MainMenu UI_MainMenu
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

    public enum ClientStatus { Disconnected, Connected, Authenticated, WaitingToGetAcceptedToPlayroom, InPlayRoom }
    public ClientStatus recordedStatus = ClientStatus.Disconnected;
    
    // is called ONLY when new scene loaded (at Start() methods of UI_1 & UI_2)
    public void ManageSceneOnLoad()
    {
        Debug.Log("Manage Scene On Load()");
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            UI_MainMenu.MainMenu_Connected(recordedStatus);
        }
        else if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            //OnlineGameManager.instance.OnPlayRoomEntered();
        }
    }
    // is called manually
    public void ManageScene(ClientStatus newStatus)
    {
        Debug.Log("Manage Scene()");
        if (recordedStatus.Equals(ClientStatus.Disconnected) && newStatus.Equals(ClientStatus.Connected))
        {
            Action act = updateUIInMainMenu;
            UnityThread.executeInUpdate(act);

            void updateUIInMainMenu() { UI_MainMenu.MainMenu_Connected(ClientStatus.Connected, false); }
        }
        else if (recordedStatus.Equals(ClientStatus.Connected) && newStatus.Equals(ClientStatus.Disconnected))
        {
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene() { UI_MainMenu.MainMenu_Connected(ClientStatus.Disconnected, false); }
        }
        else if (recordedStatus.Equals(ClientStatus.Connected) && newStatus.Equals(ClientStatus.Authenticated))
        {
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene() { UI_MainMenu.MainMenu_Connected(ClientStatus.Authenticated, true); }
        }
        else if (recordedStatus.Equals(ClientStatus.Authenticated) && newStatus.Equals(ClientStatus.Connected))
        {
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene() { UI_MainMenu.MainMenu_Connected(ClientStatus.Connected, true); }
        }
        else if (recordedStatus.Equals(ClientStatus.Authenticated) && newStatus.Equals(ClientStatus.Disconnected))
        {
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene() { UI_MainMenu.MainMenu_Connected(ClientStatus.Disconnected, true); }
        }
        else if (recordedStatus.Equals(ClientStatus.Authenticated) && newStatus.Equals(ClientStatus.InPlayRoom))
        {
            // Load play room scene
            Action act = LoadPlayRoomScene;
            UnityThread.executeInUpdate(act);

            void LoadPlayRoomScene()
            {
                OnlineGameManager.instance.OnPlayRoomEntered();
                SceneManager.LoadSceneAsync("NetworkingGameScene");
            }
        }
        else if (recordedStatus.Equals(ClientStatus.InPlayRoom) && (newStatus.Equals(ClientStatus.Authenticated) || newStatus.Equals(ClientStatus.Disconnected) || newStatus.Equals(ClientStatus.Connected)))
        {
            OnlineGameManager.instance.OnPlayRoomExited();
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene()
            {
                OnlineGameManager.instance.OnPlayRoomExited();
                SceneManager.LoadSceneAsync("MainScene");
            }

        }
        Debug.Log($"Manage Scene(); Old status [{recordedStatus}], New status [{newStatus}]");
        recordedStatus = newStatus;
    }

    private void Update()
    {
        
    }


}
