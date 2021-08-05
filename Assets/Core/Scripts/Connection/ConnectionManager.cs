using BorisUnityDev.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NetworkingMessageAttributes;
using static UI_GlobalManager;

public class ConnectionManager : MonoBehaviour
{
    // TODO MAKE CHANGES
    public static ConnectionManager instance;

    public static string ip = "18.192.64.12";

    public static int portTcp = 8384;
    public static int portUdp = 8385;

    private void Awake()
    {
        InitSingleton();
        InitConnectionCallbacks();
        UnityThread.initUnityThread();
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
    void InitConnectionCallbacks()
    {
        Connection.OnConnectedEvent += OnConnected;
        Connection.OnDisconnectedEvent += OnDisconnected;
        Connection.OnMessageReceivedEvent += OnMessageReceived;
    }
    //_____________________________________________________________________
    public void Connect()
    {
        Connection.Connect(ip, portTcp, portUdp);
    }

    public void Disconnect()
    {
        UI_GlobalManager.instance.ManageScene(ClientStatus.Disconnected);
        Connection.Disconnect();
    }

    void OnConnected(EndPoint endPoint)
    {
        UI_GlobalManager.instance.ManageScene(ClientStatus.Connected);
        Debug.Log("On Connected " + endPoint);
    }
    void OnDisconnected()
    {
        UI_GlobalManager.instance.ManageScene(ClientStatus.Disconnected);
        Debug.Log("On Disconnected " + ip);
    }
    void OnMessageReceived(string message, MessageProtocol mp)
    {
        ParseMessage(message, mp);
    }

    void ParseMessage(string msg, MessageProtocol mp)
    {
        string[] parcedMessage = msg.Split(END_OF_FILE.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        foreach(string message in parcedMessage)
        {
            if (!message.Contains(CHECK_CONNECTED))
            {
                Debug.Log($"[MESSAGE FROM SERVER]: {message}");
            }


            if (message.Equals(CLIENT_DISCONNECTED))
            {
                Disconnect();
            }
            else if (message.StartsWith(CONFIRM_ENTER_PLAY_ROOM))
            {
                string[] substrings = message.Split('|');

                Debug.Log($"Accepted to play room [{substrings[1]}]");

                UI_GlobalManager.instance.ManageScene(ClientStatus.InPlayRoom);
            }
            else if (message.StartsWith(MESSAGE_TO_ALL_CLIENTS_ABOUT_PLAYERS_DATA_IN_PLAYROOM))
            {
                if (OnlineGameManager.instance != null)
                    OnlineGameManager.instance.OnPositionMessageReceived(message);
            }
            else if (message.StartsWith(CLIENT_DISCONNECTED_FROM_THE_PLAYROOM))
            {
                if (OnlineGameManager.instance != null)
                    OnlineGameManager.instance.OnPlayerDisconnectedFromPlayroom(message);
            }
        }
        
    }
    public void ConnectToPlayroom()
    {
        // send message to server for connection, and start waiting for successful reply
        // TODO SET NICKNAME !!!
        // TODO NOW ONLY PLAYROOM NUMBER '1'

        SendMessageToServer($"{ENTER_PLAY_ROOM}|1|no_nickname", MessageProtocol.TCP);
    }
    public void LeavePlayroom()
    {
        SendMessageToServer($"{CLIENT_DISCONNECTED_FROM_THE_PLAYROOM}|1|no_nickname", MessageProtocol.TCP);
        UI_GlobalManager.instance.ManageScene(ClientStatus.Connected);
    }
    public void SendMessageToServer(string message, MessageProtocol mp)
    {
        //Debug.Log($"[{mp}] Sending message to server:" +message);
        Connection.SendMessage(message+END_OF_FILE, mp);
    }
    void OnApplicationQuit()
    {
        Disconnect();
    }
}
