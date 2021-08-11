using BorisUnityDev.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EnumsAndData;
using static NetworkingMessageAttributes;
using static UI_GlobalManager;

public class ConnectionManager : MonoBehaviour
{
    // TODO MAKE CHANGES
    public static ConnectionManager instance;

    public static string ip = "18.192.64.12";

    public static int portTcp = 8384;
    public static int portUdp = 8385;

    public UserData currentUserData;
    public ClientAccessLevel clientAccessLevel;
    bool appIsRunning = true;

    Playroom activePlayroom;

    private void Awake()
    {
        InitSingleton();
        InitConnectionCallbacksAndData();
        UnityThread.initUnityThread();

        Connection.SetConnectionValues(ip, portTcp, portUdp);
        Connect();
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
    void InitConnectionCallbacksAndData()
    {
        Connection.OnConnectedEvent += OnConnected;
        Connection.OnDisconnectedEvent += OnDisconnected;
        Connection.OnMessageReceivedEvent += OnMessageReceived;
        Connection.OnFailedToConnectEvent += OnFailededToConnectToServer;
        clientAccessLevel = ClientAccessLevel.LowestLevel;
    }
    //_____________________________________________________________________

    // Connection attempt proceeds successfull up until 12 seconds after connection,
    // to safely reconnect decided to lauch the same process after 16 seconds

    int taskToDestroy = 1;
    public async void Connect()
    {
        if (appIsRunning)
        {
            Task connectionTask = Task.Factory.StartNew(Connection.Connect);
            await connectionTask;

            Debug.Log("Connection task was destroyed: "+taskToDestroy);
            taskToDestroy++;
        }
    }

    public void Disconnect(bool notifDisconnect = true)
    {
        Connection.Disconnect(notifDisconnect);
    }
    void OnFailededToConnectToServer()
    {
        Debug.Log("ConnectionManager: OnFailedToConnectToServer -> reconnecting");
        Connect();
    }
    void OnConnected(EndPoint endPoint)
    {
        clientAccessLevel = ClientAccessLevel.LowestLevel;
        UI_GlobalManager.instance.ManageScene(ClientStatus.Connected);
        Debug.Log("On Connected " + endPoint);
    }
    void OnDisconnected()
    {
        Debug.Log("On Disconnected " + ip);
        UI_GlobalManager.instance.ManageScene(ClientStatus.Disconnected);
        Connect();
    }

    void OnMessageReceived(string message, MessageProtocol mp)
    {
        ParseMessage(message, mp);
    }

    void ParseMessage(string msg, MessageProtocol mp)
    {

        try
        {
            //if (!msg.Equals("") && !msg.Contains(CHECK_CONNECTED)) { Debug.Log($"{msg}"); }
            //if (!msg.Contains(CHECK_CONNECTED)) { Debug.Log($"[MESSAGE_FROM_SERVER][{mp}]: {msg}"); }

            // KIND OF WACKY BECAUSE UNITY VERSION OF .NET DOES NOT SUPPORT SPLIT BY STRING

            StringBuilder builder = new StringBuilder(msg);
            builder.Replace($"{END_OF_FILE}", "*");
            string res = builder.ToString();
            char[] spearator = { '*' };
            string[] parcedMessage = res.Split(spearator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string message in parcedMessage)
            {
                if (!message.Contains(CHECK_CONNECTED) && !message.Contains(MESSAGE_TO_ALL_CLIENTS_ABOUT_PLAYERS_DATA_IN_PLAYROOM) && !message.Equals("") && !message.Contains(SHOT_RESULT))
                {
                    Debug.Log($"[{mp}][MESSAGE FROM SERVER]: {message}");
                }

                if (message.Equals(CLIENT_DISCONNECTED))
                {
                    // TODO add reason
                    Debug.Log("For some reason server disconnected you");
                    Disconnect(false);
                }


                if(clientAccessLevel == ClientAccessLevel.LowestLevel)
                {
                    if (message.Contains(LOG_IN_RESULT))
                    {
                        string[] substrings = message.Split('|');
                        if (substrings[1].Equals("Success") && substrings.Length >= 3)
                        {
                            Debug.Log($"Authenticated successfully: {substrings[1]}, reading user data");

                            string[] userData = substrings[2].Split(',');
                            currentUserData = new UserData(Int32.Parse(userData[0]), userData[1], userData[2], userData[3], userData[4]);

                            Action act = StoreUserData;
                            UnityThread.executeInUpdate(act);
                            void StoreUserData()
                            {
                                PlayerPrefs.SetString(CODE_SAVED_LOGIN, currentUserData.login);
                                PlayerPrefs.SetString(CODE_SAVED_PASSWORD, currentUserData.password);
                            }

                            Debug.Log($"Current user data: {currentUserData}");
                            UI_GlobalManager.instance.ManageScene(ClientStatus.Authenticated);
                            clientAccessLevel = ClientAccessLevel.Authenticated;

                        }
                        else
                        {
                            Debug.Log($"Failed to authenticate, fail reason: {substrings[1]}");
                            Enum.TryParse(substrings[1], out RequestResult myStatus);
                            Action act = SetResult;
                            UnityThread.executeInUpdate(act);
                            void SetResult() { UI_GlobalManager.instance.SetAuthInResult(myStatus); }
                        }
                    }
                    else if (message.Contains(REGISTER_RESULT))
                    {
                        string[] substrings = message.Split('|');
                        if (substrings[1].Equals("Success") && substrings.Length >= 3)
                        {
                            Debug.Log($"Registered successfully: {substrings[1]}, reading user data");

                            string[] userData = substrings[2].Split(',');
                            currentUserData = new UserData(Int32.Parse(userData[0]), userData[1], userData[2], userData[3], userData[4]);

                            Action act = StoreUserData;
                            UnityThread.executeInUpdate(act);
                            void StoreUserData()
                            {
                                PlayerPrefs.SetString(CODE_SAVED_LOGIN, currentUserData.login);
                                PlayerPrefs.SetString(CODE_SAVED_PASSWORD, currentUserData.password);
                            }

                            Debug.Log($"Current user data: {currentUserData}");
                            UI_GlobalManager.instance.ManageScene(ClientStatus.Authenticated);
                            clientAccessLevel = ClientAccessLevel.Authenticated;

                        }
                        else
                        {
                            Debug.Log($"Failed to register, fail reason: {substrings[1]}");
                            Enum.TryParse(substrings[1], out RequestResult myStatus);
                            Action act = SetResult;
                            UnityThread.executeInUpdate(act);
                            void SetResult() { UI_GlobalManager.instance.SetRegistrationResult(myStatus); }
                        }
                    }
                }
                else if(clientAccessLevel == ClientAccessLevel.Authenticated)
                {
                    if (message.Contains(PLAYROOMS_DATA_RESPONSE))
                    {
                        UI_GlobalManager.instance.UpdatePlyroomsList(message);
                    }
                    // "reject_enter_playroom|reason_of_rejection_message|";
                    else if (message.StartsWith(REJECT_ENTER_PLAY_ROOM))
                    {
                        string[] substrings = message.Split('|');

                        Debug.Log($"Was not accepted to playroom [{substrings[1]}]");

                        UI_GlobalManager.instance.ShowLatestMessageFromServer(substrings[1]);
                        
                    }
                    // "confirm_enter_playroom|id/nameOfRoom/is_public/password/map/currentPlayers/maxPlayers"
                    else if (message.StartsWith(CONFIRM_ENTER_PLAY_ROOM))
                    {
                        string[] substrings = message.Split('|');
                        activePlayroom = new Playroom(substrings[1]);
                        Debug.Log($"Accepted to play room [{activePlayroom.id}]");

                        UI_GlobalManager.instance.ManageScene(ClientStatus.InPlayRoom);
                    }
                    else if (DoesMessageRelatedToOnlineGameManager(message))
                    {
                        if (OnlineGameManager.instance != null)
                            OnlineGameManager.instance.OnMessageFromServerRelatedToPlayroom(message);
                    }
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        
    }
    public void LogIn(string login, string password)
    {
        SendMessageToServer($"{LOG_IN}|{login}|{password}", MessageProtocol.TCP);
    }
    public void Register(string login, string password, string nickname)
    {
        SendMessageToServer($"{REGISTER}|{login}|{password}|{nickname}", MessageProtocol.TCP);
    }

    public void RequestListOfPlayrooms()
    {
        SendMessageToServer($"{PLAYROOMS_DATA_REQUEST}");
    }

    public void LeavePlayroom()
    {
        SendMessageToServer($"{CLIENT_DISCONNECTED_FROM_THE_PLAYROOM}|{activePlayroom.id}|no_nickname", MessageProtocol.TCP);
        UI_GlobalManager.instance.ManageScene(ClientStatus.Authenticated);
    }
    public void SendMessageToServer(string message, MessageProtocol mp = MessageProtocol.TCP)
    {
        if (string.IsNullOrEmpty(message)) return;

        if(mp == MessageProtocol.TCP && !message.Contains(SHOT_REQUEST))
            Debug.Log($"[{mp}] Sending message to server:" +message+END_OF_FILE);

        Connection.SendMessage(message+END_OF_FILE, mp);
    }
    void OnApplicationQuit()
    {
        if (Connection.connected)
        {
            Debug.Log("Force disconnect");
            Connection.Disconnect(false);
        }
        appIsRunning = false;
    }
}
