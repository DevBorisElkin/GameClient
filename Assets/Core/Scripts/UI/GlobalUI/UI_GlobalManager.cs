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

    public enum ClientStatus { Disconnected, Connected, WaitingToGetAcceptedToPlayroom, InPlayRoom }
    public ClientStatus recordedStatus = ClientStatus.Disconnected;
    
    // is called ONLY when new scene loaded (at Start() methods of UI_1 & UI_2)
    public void ManageSceneOnLoad()
    {
        Debug.Log("Manage Scene On Load()");
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (recordedStatus.Equals(ClientStatus.Connected))
            {
                UI_MainMenu.MainMenu_Connected(true);
            } 
            else if (recordedStatus.Equals(ClientStatus.Disconnected))
            {
                UI_MainMenu.MainMenu_Connected(false);
            }
        }
        else if (SceneManager.GetActiveScene().buildIndex == 2)
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
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene() { UI_MainMenu.MainMenu_Connected(true); }
        }
        else if (recordedStatus.Equals(ClientStatus.Connected) && newStatus.Equals(ClientStatus.Disconnected))
        {
            Action act = LoadMainMenuScene;
            UnityThread.executeInUpdate(act);

            void LoadMainMenuScene() { UI_MainMenu.MainMenu_Connected(false); }
        }
        else if (recordedStatus.Equals(ClientStatus.Connected) && newStatus.Equals(ClientStatus.InPlayRoom))
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
        else if (recordedStatus.Equals(ClientStatus.InPlayRoom) && (newStatus.Equals(ClientStatus.Connected) || newStatus.Equals(ClientStatus.Disconnected)))
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
