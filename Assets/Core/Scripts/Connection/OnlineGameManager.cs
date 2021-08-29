using BorisUnityDev.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EventManager;
using static NetworkingMessageAttributes;
using static DataTypes;
using static EnumsAndData;

public class OnlineGameManager : MonoBehaviour
{
    #region Singleton
    public static OnlineGameManager instance;
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
    #endregion

    public float pos_interpolationSpeed = 5f;
    public float rot_interpolationSpeed = 7f;
    bool inPlayRoom;

    EventManager shootingManager;

    public static string currentPlayersScores_OnEnter;
    public static string currentLobbyName_OnEnter;

    public static int maxJumpsAmount = 0;

    GameObject player;
    [HideInInspector] public PlayerMovementController playerMovementConetroller;

    UI_PlayersInLobby_Manager _ui_PlayersInLobby_Manager;
    public UI_PlayersInLobby_Manager ui_PlayersInLobby_Manager
    {
        get
        {
            if (_ui_PlayersInLobby_Manager == null)
            {
                _ui_PlayersInLobby_Manager = FindObjectOfType<UI_PlayersInLobby_Manager>();
                return _ui_PlayersInLobby_Manager;
            }
            else return _ui_PlayersInLobby_Manager;
        }
    }
    

    private void Update()
    {
        UpdatePlayersPositions();
    }
    #region callbacks
    public void SpawnPlayer(Vector3 spawnPosition)
    {
        if (SceneManager.GetActiveScene().name.Equals("NetworkingGameScene"))
        {
            player = Instantiate(PrefabsHolder.instance.player_prefab, 
                     spawnPosition, 
                     Quaternion.Euler(0, UnityEngine.Random.Range(-180, 180), 0));
            playerMovementConetroller = player.GetComponent<PlayerMovementController>();
            player.GetComponentInChildren<Player>().SetUpPlayer(new PlayerData(ConnectionManager.instance.currentUserData));
            shootingManager = FindObjectOfType<EventManager>();
        }
    }
    public void OnPlayRoomEntered()
    {
        opponents = new List<PlayerData>();
        inPlayRoom = true;
    }
    public void OnPlayRoomExited()
    {
        inPlayRoom = false;
    }
    #endregion
    #region OnPlayer_input
    public void OnPlayerMoved(Vector3 position, Vector3 rotation)
    {
        if (!sendCoordinatesToServer) return;
        string posX = position.x.ToString("0.###", CultureInfo.InvariantCulture);
        string posY = position.y.ToString("0.###", CultureInfo.InvariantCulture);
        string posZ = position.z.ToString("0.###", CultureInfo.InvariantCulture);

        string rotX = rotation.x.ToString("0.###", CultureInfo.InvariantCulture);
        string rotY = rotation.y.ToString("0.###", CultureInfo.InvariantCulture);
        string rotZ = rotation.z.ToString("0.###", CultureInfo.InvariantCulture);

        ConnectionManager.instance.SendMessageToServer($"{CLIENT_SHARES_PLAYROOM_POSITION}|" +
            $"{posX}/{posY}/{posZ}|{rotX}/{rotY}/{rotZ}", MessageProtocol.UDP);
    }
    #endregion
    #region IncomingMessages

    public void OnMessageFromServerRelatedToPlayroom(string message)
    {
        if (message.StartsWith(MESSAGE_TO_ALL_CLIENTS_ABOUT_PLAYERS_DATA_IN_PLAYROOM))
        {
            OnPositionMessageReceived(message);
        }
        else if (message.StartsWith(CLIENT_DISCONNECTED_FROM_THE_PLAYROOM))
        {
            OnPlayerDisconnectedFromPlayroom(message);
        }
        else if (message.StartsWith(SHOT_RESULT))
        {
            OnShotMessageReceived(message);
        }else if (message.StartsWith(JUMP_RESULT))
        {
            string[] msg = message.Split('|');
            int currentJumpsAmount = Int32.Parse(msg[1]);
            OnJumpMessageReceived(currentJumpsAmount);
        }
        // "jump_amount|2|true // 2 = current available amount of jumps // true = setAfterRevive
        else if (message.StartsWith(JUMP_AMOUNT))
        {
            string[] msg = message.Split('|');
            int currentJumpsAmount = Int32.Parse(msg[1]);
            bool setJumpAmountAfterRevive = bool.Parse(msg[2]);
            OnJumpsAmountMessageReceived(currentJumpsAmount, setJumpAmountAfterRevive);
        }
        else if (message.StartsWith(PLAYERS_SCORES_IN_PLAYROOM))
        {
            OnReceivedPlayersScores(message);
        }
        else if (message.StartsWith(PLAYER_REVIVED))
        {
            OnMessagePlayerRevived(message);
        }else if (message.StartsWith(SPAWN_DEATH_PARTICLES))
        {
            MessageParser.ParseOnSpawnDeathParticlesMessage(message, out Vector3 spawnPosition, out Quaternion spawnRotation);
            UnityThread.executeInUpdate(() => {
                Instantiate( PrefabsHolder.instance.playerDeathParticles_prefab, spawnPosition, spawnRotation);
            });
        }
    }

    public void OnPlayerDisconnectedFromPlayroom(string message)
    {
        if (!inPlayRoom) return;
        string[] substrings = message.Split('|');
        string leftPlayerIp = substrings[3];

        PlayerData leftPlayer = FindPlayerByIp(leftPlayerIp);
        if (leftPlayer != null)
        {
            leftPlayer.playerLeft = true;

            Action act = CheckRemoveAndDeleteLeftPlayers;
            UnityThread.executeInUpdate(act);
        }
    }

    public void OnPositionMessageReceived(string message)
    {
        if (!inPlayRoom) return;

        List<PlayerData> retrievedPlayerData = MessageParser.ParseOnPositionsMessage(message);
        if (retrievedPlayerData == null || retrievedPlayerData.Count <= 0) return;

        bool noticedUnspanedPlayers = false;
        foreach (PlayerData a in retrievedPlayerData)
        {
            PlayerData correctPlayer = FindPlayerByIp(a.ip);
            if(correctPlayer != null)
            {
                correctPlayer.position = a.position;
                correctPlayer.rotation = a.rotation;
            }
            else
            {
                PlayerData newlyCreatedPlayer = new PlayerData();
                newlyCreatedPlayer.nickname = a.nickname;
                newlyCreatedPlayer.ip = a.ip;
                newlyCreatedPlayer.position = a.position; // TODO No initial connection of position with server
                newlyCreatedPlayer.rotation = a.rotation;
                opponents.Add(newlyCreatedPlayer);

                noticedUnspanedPlayers = true;
            }
        }
        if (noticedUnspanedPlayers)  UnityThread.executeInUpdate(() => { CheckUnspawnedPlayers(); });
    }
    // code|posOfShootingPoint|rotationAtRequestTime|ipOfShootingPlayer
    // "shot_result|123/45/87|543/34/1|198.0.0.126";
    public void OnShotMessageReceived(string message)
    {
        if (!inPlayRoom) return;
        MessageParser.ParseOnShotMessage(message, out Vector3 position, out Quaternion rotation, out string ip);

        GameObject objToIgnore;
        // we know that it's our player shoots
        if (ip.Equals(ConnectionManager.instance.currentUserData.ip))
        {
            objToIgnore = player;
            Action actForbidToShoot = playerMovementConetroller.ForbidToShootFromServer;
            UnityThread.executeInUpdate(actForbidToShoot);
        }
        else
        {
            PlayerData plData = FindPlayerByIp(ip);
            if (plData != null && plData.controlledGameObject != null)
                objToIgnore = plData.controlledGameObject;
            else return;
        }
        Action act = Action;
        UnityThread.executeInUpdate(act);
        void Action() => shootingManager.MakeActualShot(position, rotation, objToIgnore, ip);
    }

    public void OnJumpMessageReceived(int currentJumps)
    {
        if (!inPlayRoom) return;

        Action act = Action;
        UnityThread.executeInUpdate(act);
        void Action()
        {
            playerMovementConetroller.MakeJumpOnline();
            playerMovementConetroller.SetLocalAmountOfJumps(currentJumps);
        }
    }
    public void OnJumpsAmountMessageReceived(int currentJumps, bool resetAfterRevive)
    {
        if (!inPlayRoom) return;

        if (!resetAfterRevive) 
            UnityThread.executeInUpdate(() => { playerMovementConetroller.SetLocalAmountOfJumps(currentJumps); });
        else 
            playerMovementConetroller.SetAmountOfJumps(currentJumps); // resetting after revive
    }

    public void TryToShootOnline(Vector3 projectileSpawnPoint, Vector3 angleForProjectile)
    {
        //Debug.Log("Our player is trying to shoot online");
        string posX = projectileSpawnPoint.x.ToString("0.###", CultureInfo.InvariantCulture);
        string posY = projectileSpawnPoint.y.ToString("0.###", CultureInfo.InvariantCulture);
        string posZ = projectileSpawnPoint.z.ToString("0.###", CultureInfo.InvariantCulture);

        string rotX = angleForProjectile.x.ToString("0.###", CultureInfo.InvariantCulture);
        string rotY = angleForProjectile.y.ToString("0.###", CultureInfo.InvariantCulture);
        string rotZ = angleForProjectile.z.ToString("0.###", CultureInfo.InvariantCulture);

        ConnectionManager.instance.SendMessageToServer($"{SHOT_REQUEST}|{posX}/{posY}/{posZ}|" +
            $"{rotX}/{rotY}/{rotZ}", MessageProtocol.TCP);
    }
    #endregion
    #region System
    // basically checks if player that joined room has not been yet added as an GameObject
    Vector3 spawnDefaultPos = new Vector3(0, 100, 0);
    void CheckUnspawnedPlayers()
    {
        foreach (PlayerData a in opponents)
        {
            if (a.controlledGameObject == null)
            {
                try
                {
                    a.controlledGameObject = Instantiate(PrefabsHolder.instance.opponent_prefab, spawnDefaultPos, a.rotation);
                    a.controlledGameObject.transform.position = spawnDefaultPos;
                    Debug.Log($"Spawned opponent at position {a.controlledGameObject.transform.position}");
                    a.controlledGameObject.GetComponentInChildren<Player>().SetUpPlayer(a);
                }
                catch (Exception e) { Debug.LogError(e.Message + " " + e.StackTrace); }
            }
        }
    }

    void CheckRemoveAndDeleteLeftPlayers()
    {
        try
        {
            List<PlayerData> playersToDelete = new List<PlayerData>();

            foreach (PlayerData a in opponents)
            {
                if (a.controlledGameObject != null && a.playerLeft)
                    playersToDelete.Add(a);
            }

            foreach (PlayerData a in playersToDelete)
            {
                Destroy(a.controlledGameObject);
                opponents.Remove(a);
            }
        }
        catch (Exception e) { Debug.LogError(e.Message + " " + e.StackTrace); }
    }

    List<PlayerData> opponents;

    public class PlayerData
    {
        public PlayerData() {}
        public PlayerData(UserData userData) 
        {
            nickname = userData.nickname;
        }

        public string nickname;
        public string ip;

        public bool playerLeft;

        public Vector3 position;
        public Quaternion rotation;

        public GameObject controlledGameObject;
    }

    public PlayerData FindPlayerByIp(string ip)
    {
        foreach (PlayerData a in opponents)
        {
            if (a.ip.Equals(ip)) return a;
        }
        return null;
    }
    public float interpolateIfDiscanceGreater = 7f;
    void UpdatePlayersPositions()
    {
        if (!inPlayRoom) return;
        if (opponents!= null && opponents.Count > 0)
        {
            try
            {
                foreach (PlayerData a in opponents)
                {
                    if (a != null && a.controlledGameObject != null)
                    {
                        // check if player changes position too fast, prefered to teleport him instead of interpolating
                        if(Vector3.Distance(a.controlledGameObject.transform.position, a.position) > interpolateIfDiscanceGreater)
                        {
                            if (!a.position.Equals(Vector3.zero))
                            {
                                Debug.Log($"Teleported player {a.ip} {a.nickname}, initial pos was {a.controlledGameObject.transform.position}, result pos became {a.position}");
                                a.controlledGameObject.transform.position = a.position;
                                a.controlledGameObject.transform.rotation = a.rotation;
                            }
                        }
                        else
                        {
                            a.controlledGameObject.transform.position = Vector3.Lerp(a.controlledGameObject.transform.position, a.position, Time.deltaTime * pos_interpolationSpeed);
                            a.controlledGameObject.transform.rotation = Quaternion.Lerp(a.controlledGameObject.transform.rotation, a.rotation, Time.deltaTime * rot_interpolationSpeed);
                        }
                        
                    }
                }
            }
            catch (Exception e) { Debug.LogError(e.Message + " " + e.StackTrace); }
        }
    }
    #endregion

    #region Players scores

    // players_scores|data@data@data
    // {fullFataOfPlayersInThatRoom} => ip/nickname/kills/deaths@ip/nickname/kills/deaths@ip/nickname/kills/deaths
    public void OnReceivedPlayersScores(string message)
    {
        string[] msg = message.Split('|');
        
        Action act = Action;
        UnityThread.executeInUpdate(act);
        void Action() { ui_PlayersInLobby_Manager.SpawnLobbyItems(msg[1]); }
    }

    #endregion

    #region Revive player

    // "player_revived|0/0/0|current_amount_of_jumps
    public void OnMessagePlayerRevived(string message)
    {
        string[] msg = message.Split('|');
        string[] coordinates = msg[1].Split('/');
        try
        {
            Vector3 spawnPosition = new Vector3(
                float.Parse(coordinates[0], CultureInfo.InvariantCulture),
                float.Parse(coordinates[1], CultureInfo.InvariantCulture),
                float.Parse(coordinates[2], CultureInfo.InvariantCulture)
                );
            int currentJumpsAmount = Int32.Parse(msg[2], CultureInfo.InvariantCulture);

            UnityThread.executeInUpdate(() => {
                playerMovementConetroller.RevivePlayer(spawnPosition, currentJumpsAmount);
            });
        }
        catch(Exception e) { Debug.Log(e); }

    }

    #endregion



}
