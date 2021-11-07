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
using static OpponentPointer;

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

    [HideInInspector] public GameObject playerGameObj;
    [HideInInspector] public Player player;
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
            playerGameObj = Instantiate(PrefabsHolder.instance.player_prefab, 
                     spawnPosition, 
                     Quaternion.Euler(0, UnityEngine.Random.Range(-180, 180), 0));
            playerMovementConetroller = playerGameObj.GetComponent<PlayerMovementController>();
            player = playerGameObj.GetComponentInChildren<Player>();
            player.SetUpPlayer(new PlayerData(ConnectionManager.instance.currentUserData));
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
        try
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
            }
            else if (message.StartsWith(JUMP_RESULT))
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
            }
            else if (message.StartsWith(SPAWN_DEATH_PARTICLES))
            {
                MessageParser.ParseOnSpawnDeathParticlesMessage(message, out Vector3 spawnPosition, out Quaternion spawnRotation);
                UnityThread.executeInUpdate(() => {
                    Instantiate(PrefabsHolder.instance.playerDeathParticles_prefab, spawnPosition, spawnRotation);
                });
            }
            else if (message.Contains(PLAYER_WAS_KILLED_MESSAGE))
            {
                UnityThread.executeInUpdate(() => {
                    if (UI_InGameMsgEventsManager.instance != null)
                    {
                        int deadPlayerId = UI_InGameMsgEventsManager.instance.FromServer_DeathEventMessageReceived(message);
                        DisableXrayForOpponent(deadPlayerId);

                        if (deadPlayerId == ConnectionManager.instance.currentUserData.db_id)
                        {
                            player.ResetAllRuneEffects();
                            player.ResetAllDebuffEffects();
                        }
                        else
                        {
                            var opponent = FindPlayerByDbId(deadPlayerId);
                            if (opponent != null)
                            {
                                opponent.player.ResetAllRuneEffects();
                                opponent.player.ResetAllDebuffEffects();
                            }
                        }
                    }
                });
            }
            else if (message.Contains(CLIENT_CONNECTED_TO_THE_PLAYROOM))
            {
                string[] substrings = message.Split('|');
                UnityThread.executeInUpdate(() => {
                    if (UI_InGameMsgEventsManager.instance != null)
                    {
                        UI_InGameMsgEventsManager.instance.FromServer_PlayerJoinedPlayroomMessageReceived(substrings[2]);
                    }
                });
            }
            else if (message.Contains(MATCH_STARTED_FORCE_OVERRIDE_POSITION_AND_JUMPS))
            {
                Debug.Log(message);

                string[] substrings = message.Split('|');
                Debug.Log(substrings[1]);
                int newJumpsAmount = Int32.Parse(substrings[1]);
                string[] position = substrings[2].Split('/');
                Vector3 spawnPosition = new Vector3(
                    float.Parse(position[0], CultureInfo.InvariantCulture),
                    float.Parse(position[1], CultureInfo.InvariantCulture),
                    float.Parse(position[2], CultureInfo.InvariantCulture)
                    );
                UnityThread.executeInUpdate(() => {
                    UI_InGame.instance.OnNewMatchState(MatchState.InGame);
                    playerMovementConetroller.RevivePlayer(spawnPosition, newJumpsAmount); 
                
                });
            }
            else if (message.Contains(MATCH_TIME_REMAINING))
            {
                UnityThread.executeInUpdate(() => { 
                    if (UI_InGame.instance != null) UI_InGame.instance.UpdateTimeLeftTxt(Int32.Parse(message.Split('|')[1]));
                });
            }
            else if (message.Contains(MATCH_FINISHED))
            {
                UnityThread.executeInUpdate(() =>
                {
                    EventManager.isAlive = false;
                    string[] substrings = message.Split('|');
                    ConnectionManager.activePlayroom.matchState = MatchState.Finished;
                    ConnectionManager.activePlayroom.winnerNickname = substrings[2];
                    Enum.TryParse(substrings[3], out MatchResult _res);
                    ConnectionManager.activePlayroom.matchResult = _res;

                    UI_InGame.instance.OnMatchResult(_res);
                });
            }
            else if (message.Contains(RUNE_SPAWNED))
            {
                MessageParser.ParseOnRuneSpawnedMessage(message, out Vector3 spawnPosition, out Rune runeType, out int runeId);
                UnityThread.executeInUpdate(() =>
                {
                    GameObject spawnedRune = Instantiate(PrefabsHolder.instance.rune_prefab, spawnPosition, Quaternion.identity);
                    RuneInstance rune = spawnedRune.GetComponent<RuneInstance>();
                    rune.SetUpRune(runeId, runeType);
                });
            }
            else if (message.Contains(RUNE_PICKED_UP))
            {
                MessageParser.ParseOnRunePickedUpMessage(message, out int runeId, out int playerWhoPicked_db_id, out Rune rune, out string nickOfPicker, out float effectDuration);
                UnityThread.executeInUpdate(() =>
                {
                    // 1) Add rune effect on player
                    if(playerWhoPicked_db_id == ConnectionManager.instance.currentUserData.db_id)
                    {
                        player.AddRuneEffect(rune);
                    }
                    else
                    {
                        var opponent =  FindPlayerByDbId(playerWhoPicked_db_id);
                        if(opponent != null)
                        {
                            opponent.player.AddRuneEffect(rune);
                        }
                    }

                    // 2) Destroy rune instance
                    List<GameObject> runesToDestroy = new List<GameObject>();

                    RuneInstance[] runes = FindObjectsOfType<RuneInstance>();
                    foreach (var a in runes)
                        if (a.runeId == runeId) runesToDestroy.Add(a.gameObject);

                    foreach (var a in runesToDestroy)
                        Destroy(a);

                    // 3) create UI notifiying of picking up the rune
                    UI_InGameMsgEventsManager.instance.FromServer_PlayerPickedUpRune(rune, nickOfPicker);

                });
            }
            else if (message.Contains(RUNE_EFFECT_EXPIRED))
            {
                MessageParser.ParseOnRuneEffectExpiredMessage(message, out int affectedPlayerDbId, out Rune runeType);
                UnityThread.executeInUpdate(() =>
                {
                    if (affectedPlayerDbId == ConnectionManager.instance.currentUserData.db_id)
                    {
                        player.RemoveRuneEffect(runeType);
                    }
                    else
                    {
                        var opponent = FindPlayerByDbId(affectedPlayerDbId);
                        if (opponent != null)
                        {
                            opponent.player.RemoveRuneEffect(runeType);
                        }
                    }
                });
            }else if (message.Contains(RUNES_INFO))
            {
                List<SpawnedRuneInstance> spawnedRuneInstances = MessageParser.ParseOnRunesInfoMessage(message);
                if (spawnedRuneInstances.Count == 0) return;
                UnityThread.executeCoroutine(DelayedRunesInfoCoroutine());
                
                IEnumerator DelayedRunesInfoCoroutine()
                {
                    yield return new WaitForSeconds(1f);
                    foreach (var a in spawnedRuneInstances)
                    {
                        GameObject spawnedRune = Instantiate(PrefabsHolder.instance.rune_prefab, a.position, Quaternion.identity);
                        RuneInstance rune = spawnedRune.GetComponent<RuneInstance>();
                        rune.SetUpRune(a.uniqueId, a.runeType);
                    }
                }
            }
            else if (message.Contains(RUNE_EFFECTS_INFO))
            {
                List<RuneEffectInfo> runeEffectsInfo = MessageParser.ParseOnRuneEffectsMessage(message);
                if (runeEffectsInfo.Count == 0) return;
                UnityThread.executeCoroutine(DelayedRuneEffectsCoroutine());

                IEnumerator DelayedRuneEffectsCoroutine()
                {
                    yield return new WaitForSeconds(1f);
                    foreach (var a in runeEffectsInfo)
                    {
                        if (a.runeEffects.Count == 0) continue;

                        if (a.playerDbId == ConnectionManager.instance.currentUserData.db_id)
                        {
                            foreach (var b in a.runeEffects)
                                player.AddRuneEffect(b);
                        }
                        else
                        {
                            var opponent = FindPlayerByDbId(a.playerDbId);
                            if (opponent != null)
                            {
                                foreach (var b in a.runeEffects)
                                    opponent.player.AddRuneEffect(b);
                            }
                        }
                    }
                }
            }
            else if (message.Contains(PLAYER_RECEIVED_DEBUFF))
            {
                MessageParser.ParseOnDebuffsMessage(message, out int playerDbId, out Rune debuff);

                UnityThread.executeInUpdate(() =>
                {
                    // 1) Add rune effect on player
                    if (playerDbId == ConnectionManager.instance.currentUserData.db_id)
                        player.AddDebuffEffect(debuff);
                    else
                    {
                        var opponent = FindPlayerByDbId(playerDbId);
                        if (opponent != null)
                        {
                            opponent.player.AddDebuffEffect(debuff);
                        }
                    }
                });
            }
            else if (message.Contains(PLAYER_DEBUFF_ENDED))
            {
                MessageParser.ParseOnDebuffsMessage(message, out int playerDbId, out Rune debuff);
                UnityThread.executeInUpdate(() =>
                {
                    // 1) Add rune effect on player
                    if (playerDbId == ConnectionManager.instance.currentUserData.db_id)
                    {
                        player.RemoveDebuffEffect(debuff);
                    }
                    else
                    {
                        var opponent = FindPlayerByDbId(playerDbId);
                        if (opponent != null)
                        {
                            opponent.player.RemoveDebuffEffect(debuff);
                        }
                    }
                });
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
    public void OnPlayerDisconnectedFromPlayroom(string message)
    {
        if (!inPlayRoom) return;
        string[] substrings = message.Split('|');
        int leftPlayerDbId = Int32.Parse(substrings[3]);

        PlayerData leftPlayer = FindPlayerByDbId(leftPlayerDbId);
        if (leftPlayer != null)
        {
            UnityThread.executeInUpdate(() => 
            {
                if (CameraRenderingManager.instance != null) CameraRenderingManager.instance.opponentPointerHandler.DestroyPointer(leftPlayer.opponentPointer);
                UI_InGameMsgEventsManager.instance.FromServer_PlayerExitedPlayroomMessageReceived(leftPlayer.nickname);
                leftPlayer.playerLeft = true;
                CheckRemoveAndDeleteLeftPlayers();

            });
        }
    }
    // "players_positions_in_playroom|nickname,db_id,position,rotation@nickname,db_id,position,rotation@enc..."
    public void OnPositionMessageReceived(string message)
    {
        if (!inPlayRoom) return;

        List<PlayerData> retrievedPlayerData = MessageParser.ParseOnPositionsMessage(message);
        if (retrievedPlayerData == null || retrievedPlayerData.Count <= 0) return;

        bool noticedUnspanedPlayers = false;
        foreach (PlayerData a in retrievedPlayerData)
        {
            PlayerData correctPlayer = FindPlayerByDbId(a.db_id);
            if(correctPlayer != null)
            {
                correctPlayer.position = a.position;
                correctPlayer.rotation = a.rotation;
            }
            else
            {
                PlayerData newlyCreatedPlayer = new PlayerData();
                newlyCreatedPlayer.nickname = a.nickname;
                newlyCreatedPlayer.db_id = a.db_id;
                newlyCreatedPlayer.position = a.position; // TODO No initial connection of position with server
                newlyCreatedPlayer.rotation = a.rotation;
                opponents.Add(newlyCreatedPlayer);

                noticedUnspanedPlayers = true;
            }
        }
        if (noticedUnspanedPlayers)  UnityThread.executeInUpdate(() => { CheckUnspawnedPlayers(); });
    }
    // code|posOfShootingPoint|rotationAtRequestTime|dbIdOfShootingPlayer
    // "shot_result|123/45/87|543/34/1|13";
    public void OnShotMessageReceived(string message)
    {
        if (!inPlayRoom) return;
        MessageParser.ParseOnShotMessage(message, out Vector3 position, out Quaternion rotation, out int db_id, out List<Rune> activeRuneModifiers);

        GameObject objToIgnore;
        // we know that it's our player shoots
        if (db_id.Equals(ConnectionManager.instance.currentUserData.db_id))
        {
            objToIgnore = playerGameObj;
            Action actForbidToShoot = playerMovementConetroller.ForbidToShootFromServer;
            UnityThread.executeInUpdate(actForbidToShoot);
        }
        else
        {
            PlayerData plData = FindPlayerByDbId(db_id);
            if (plData != null && plData.controlledGameObject != null)
                objToIgnore = plData.controlledGameObject;
            else return;
        }
        Action act = Action;
        UnityThread.executeInUpdate(act);
        void Action() => shootingManager.MakeActualShot(position, rotation, objToIgnore, db_id, activeRuneModifiers);
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

    #region Outcoming messages
    public void SendMessage_PlayerTriesToPickUpRune(int runeId, Rune runeType)
    {
        string message = $"{RUNE_TRY_TO_PICK_UP}|{ConnectionManager.instance.currentUserData.db_id}|{runeType}|{runeId}";
        ConnectionManager.instance.SendMessageToServer(message, MessageProtocol.TCP);
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
                    
                    a.controlledGameObject = Instantiate(PrefabsHolder.instance.opponent_prefab, a.position, a.rotation);
                    a.controlledGameObject.transform.position = a.position;
                    Debug.Log($"Spawned opponent at position {a.controlledGameObject.transform.position}");
                    if (a.player == null) a.player = a.controlledGameObject.GetComponentInChildren<Player>();
                    a.player.SetUpPlayer(a);
                }
                catch (Exception e) { Debug.LogError(e.Message + " " + e.StackTrace); }
            }
        }
        UI_InGame.instance.UpdateWaitingForPlayersText(opponents.Count);
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

            UI_InGame.instance.UpdateWaitingForPlayersText(opponents.Count);
        }
        catch (Exception e) { Debug.LogError(e.Message + " " + e.StackTrace); }
    }

    public void DisableXrayForOpponent(int opponentId)
    {
        foreach(var a in opponents)
        {
            if(a.db_id == opponentId)
            {
                a.lastTimeDead = DateTime.Now;
                break;
            }
        }
    }

    public List<PlayerData> opponents;

    public class PlayerData
    {
        public PlayerData() {}
        public PlayerData(UserData userData) 
        {
            nickname = userData.nickname;
        }

        public string nickname;
        public int db_id;

        public bool playerLeft;

        public Vector3 position;
        public Quaternion rotation;

        public GameObject controlledGameObject;

        public DateTime lastTimeDead;

        public Player player;
        public Pointer opponentPointer;
    }

    public PlayerData FindPlayerByDbId(int dbId)
    {
        foreach (PlayerData a in opponents)
        {
            if (a.db_id.Equals(dbId)) return a;
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
                                Debug.Log($"Teleported player {a.db_id} {a.nickname}, initial pos was {a.controlledGameObject.transform.position}, result pos became {a.position}");
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
        void Action() 
        { 
            if(ui_PlayersInLobby_Manager != null)
            ui_PlayersInLobby_Manager.SpawnLobbyItems(msg[1]); 
        }
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
