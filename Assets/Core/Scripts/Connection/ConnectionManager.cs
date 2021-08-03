using BorisUnityDev.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NetworkingMessageAttributes;

public class ConnectionManager : MonoBehaviour
{
    // TODO MAKE CHANGES
    public static ConnectionManager instance;
    UI_TestConnectionPanel _ui_testConnect;

    public UI_TestConnectionPanel UI_TestConnect
    {
        get
        {
            if (_ui_testConnect == null)
            {
                _ui_testConnect = FindObjectOfType<UI_TestConnectionPanel>();
                return _ui_testConnect;
            }
            else return _ui_testConnect;
        }
     }

    Connection connection;

    public static string ip = "18.192.64.12";

    public static int portTcp = 8384;
    public static int portUdp = 8385;

    private void Awake()
    {
        InitSingleton();
        InitConnection();

        currentStatus = ClientCurrentStatus.Disconnected;

        UnityThread.initUnityThread();
        
        _ui_testConnect = FindObjectOfType<UI_TestConnectionPanel>();
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
    void InitConnection()
    {
        connection = new Connection();
        connection.OnConnectedEvent += OnConnected;
        connection.OnDisconnectedEvent += OnDisconnected;
        connection.OnMessageReceivedEvent += OnMessageReceived;
    }
    public void Connect()
    {
        connection.Connect(ip, portTcp);
        UDP.ConnectTo(ip, portUdp, connection);
    }

    public void Disconnect()
    {
        if (currentStatus.Equals(ClientCurrentStatus.InPlayRoom))
            LeavePlayroom();


        connection.Disconnect();
        UDP.Disconnect();
    }

    void OnConnected(EndPoint endPoint)
    {
        currentStatus = ClientCurrentStatus.Connected;

        Debug.Log("On Connected " + endPoint);

        Action act = Action1;
        UnityThread.executeInUpdate(act);

        void Action1()
        {
            UI_TestConnect.OnMessageReceived($"[SERVER_CONNECTED][{ip}]");
            UI_TestConnect.OnConnectedCallback(endPoint.ToString());
        }
    }
    void OnDisconnected()
    {
        currentStatus = ClientCurrentStatus.Disconnected;

        Debug.Log("On Disconnected " + ip);

        Action act = Action2;
        UnityThread.executeInUpdate(act);

        void Action2()
        {
            //UI_TestConnect.OnMessageReceived($"[SERVER_DISCONNECTED][{ip}]");
            UI_TestConnect.serverOutputText.text = "";
            UI_TestConnect.OnDisconnectedCallback();
        }
    }
    void OnMessageReceived(string message, MessageProtocol mp)
    {
        //if(!mp.Equals(MessageProtocol.UDP))
        Debug.Log($"On message received[{mp}]: "+message);

        if (currentStatus.Equals(ClientCurrentStatus.Connected) || currentStatus.Equals(ClientCurrentStatus.WaitingToGetAcceptedToPlayroom))
        {
            Action act = Action3;
            UnityThread.executeInUpdate(act);

            void Action3()
            {
                UI_TestConnect.OnMessageReceived($"[SERVER_MESSAGE][{mp}][{ip}]: {message}");
            }

            ParseMessage(message, mp);
        }
        else if (currentStatus.Equals(ClientCurrentStatus.InPlayRoom))
        {
            ParseMessage(message, mp);
        }

        // TODO CHECK MESSAGES
    }

    void ParseMessage(string message, MessageProtocol mp)
    {
        if (message.StartsWith(CONFIRM_ENTER_PLAY_ROOM))
        {
            if (!currentStatus.Equals(ClientCurrentStatus.WaitingToGetAcceptedToPlayroom)) return;

            string[] substrings = message.Split('|');

            Debug.Log($"Accepted to play room [{substrings[1]}]");

            Action act = OnConnectedToPlayroom;
            UnityThread.executeInUpdate(act);
            //OnConnectedToPlayroom();
        }else if (message.StartsWith(MESSAGE_TO_ALL_CLIENTS_ABOUT_PLAYERS_DATA_IN_PLAYROOM))
        {
            if (!currentStatus.Equals(ClientCurrentStatus.InPlayRoom)) return;

            // TODO Send this to special class to handle players gameobjects

            if(OnlineGameManager.instance != null)
                OnlineGameManager.instance.OnPositionMessageReceived(message);


        }else if (message.StartsWith(CLIENT_DISCONNECTED_FROM_THE_PLAYROOM))
        {
            if (!currentStatus.Equals(ClientCurrentStatus.InPlayRoom)) return;

            if (OnlineGameManager.instance != null)
                OnlineGameManager.instance.OnPlayerDisconnectedFromPlayroom(message);
        }
    }
    // __________________________________________________________________

    public enum ClientCurrentStatus { Disconnected, Connected, WaitingToGetAcceptedToPlayroom, InPlayRoom}
    public ClientCurrentStatus currentStatus;
    
    public void OnClick_ConnectToPlayroom()
    {
        Debug.Log("OnClick_ConnectToPlayroom();");
        ConnectToPlayroom();
    }

    public void OnClick_LeavePlayroom()
    {
        Debug.Log("OnClick_LeavePlayroom();");
        LeavePlayroom();
    }

    void ConnectToPlayroom()
    {
        // send message to server for connection, and start waiting for successful reply
        // TODO SET NICKNAME !!!
        // TODO NOW ONLY PLAYROOM NUMBER '1'

        SendMessageToServer($"{ENTER_PLAY_ROOM}|1|no_nickname", MessageProtocol.TCP);
        currentStatus = ClientCurrentStatus.WaitingToGetAcceptedToPlayroom;
    }
    void LeavePlayroom()
    {
        //string myAddress = connection.socket.LocalEndPoint.ToString().Split(':')[0];
        SendMessageToServer($"{CLIENT_DISCONNECTED_FROM_THE_PLAYROOM}|1|no_nickname", MessageProtocol.TCP);
        currentStatus = ClientCurrentStatus.Connected;

        SceneManager.LoadSceneAsync("MainScene");
    }

    void OnConnectedToPlayroom()
    {
        // open playroom scene
        // and start working with playroom messages

        OnlineGameManager.instance.OnPlayRoomEntered();
        currentStatus = ClientCurrentStatus.InPlayRoom;

        SceneManager.LoadSceneAsync("NetworkingGameScene");
    }

    public void SendMessageToServer(string message, MessageProtocol mp)
    {
        //Debug.Log($"[{mp}] Sending message to server:" +message);
        connection.SendMessage(message, mp);
    }


    void OnApplicationQuit()
    {
        Disconnect();
    }
}
