using BorisUnityDev.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NetworkingMessageAttributes;

public class OnlineGameManager : MonoBehaviour
{
    public static OnlineGameManager instance;

    public Vector3 spawnPosition;
    public float pos_interpolationSpeed = 5f;
    public float rot_interpolationSpeed = 7f;
    bool inPlayRoom;

    ShootingManager shootingManager;

    GameObject player;
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

    private void Update()
    {
        UpdatePlayersPositions();
    }
    #region callbacks
    public void SpawnPlayer()
    {
        if (SceneManager.GetActiveScene().name.Equals("NetworkingGameScene"))
        {
            player = Instantiate(PrefabsHolder.instance.player_prefab, spawnPosition, Quaternion.identity);
            player.GetComponentInChildren<Player>().SetUpPlayer(new PlayerData(ConnectionManager.instance.currentUserData));
            shootingManager = FindObjectOfType<ShootingManager>();
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
        string posX = position.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        string posY = position.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        string posZ = position.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

        string rotX = rotation.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        string rotY = rotation.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        string rotZ = rotation.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

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

    // "players_positions_in_playroom|nickname,ip,position,rotation@nickname,ip,position,rotation@enc..."
    public void OnPositionMessageReceived(string message)
    {
        //Debug.Log($"{message}|>inPlayRoom: {inPlayRoom}");
        if (!inPlayRoom) return;
        try
        {
            if (message.Length <= "players_positions_in_playroom|".Length) return;

            int firstIndex = message.IndexOf(MESSAGE_TO_ALL_CLIENTS_ABOUT_PLAYERS_DATA_IN_PLAYROOM);
            int lastIndex = message.LastIndexOf(MESSAGE_TO_ALL_CLIENTS_ABOUT_PLAYERS_DATA_IN_PLAYROOM);

            if (firstIndex != lastIndex)
            {  // 2 or more occourences, removing all except the first one

                int countedLenght = MESSAGE_TO_ALL_CLIENTS_ABOUT_PLAYERS_DATA_IN_PLAYROOM.Length;
                string tmp = message.Substring(countedLenght + 1);
                int indexOfSecondOccourence = tmp.IndexOf(MESSAGE_TO_ALL_CLIENTS_ABOUT_PLAYERS_DATA_IN_PLAYROOM);
                int realIndexOfSecOcc = indexOfSecondOccourence + countedLenght + 1;
                message = message.Substring(0, realIndexOfSecOcc);
            }


            string[] substrings = message.Split('|');
            string[] playersData = substrings[1].Split('@');

            bool noticedUnspanedPlayers = false;
            foreach (string a in playersData)
            {
                string[] subdata = a.Split(',');

                PlayerData correctPlayer = FindPlayerByIp(subdata[1]);

                string[] coordinatesXYZ = subdata[2].Split('/');
                string[] rotation = subdata[3].Split('/');

                if (correctPlayer != null)
                {
                    correctPlayer.position = new Vector3(float.Parse(coordinatesXYZ[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(coordinatesXYZ[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(coordinatesXYZ[2], System.Globalization.CultureInfo.InvariantCulture));
                    correctPlayer.rotation = Quaternion.Euler(float.Parse(rotation[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(rotation[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(rotation[2], System.Globalization.CultureInfo.InvariantCulture));
                    // updating that player's position
                    // ...

                }
                else
                {
                    Debug.Log("Noticed a player in playroom that has not beed added yet.");

                    PlayerData newlyCreatedPlayer = new PlayerData();
                    newlyCreatedPlayer.nickname = subdata[0];
                    newlyCreatedPlayer.ip = subdata[1];
                    newlyCreatedPlayer.position = new Vector3(float.Parse(coordinatesXYZ[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(coordinatesXYZ[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(coordinatesXYZ[2], System.Globalization.CultureInfo.InvariantCulture));
                    newlyCreatedPlayer.rotation = Quaternion.Euler(float.Parse(rotation[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(rotation[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(rotation[2], System.Globalization.CultureInfo.InvariantCulture));

                    opponents.Add(newlyCreatedPlayer);

                    noticedUnspanedPlayers = true;
                }

            }
            if (noticedUnspanedPlayers)
            {
                Action act = CheckUnspawnedPlayers;
                UnityThread.executeInUpdate(act);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("_____EXCEPTION_____");
            Debug.LogError($"EXCEPTION WORKED BECAUSE OF MESSAGE\n{message}");
            Debug.LogError(e.Message + " " + e.StackTrace);
            Debug.LogError("_____EXCEPTION_____");

        }
    }
    // code|posOfShootingPoint|rotationAtRequestTime|ipOfShootingPlayer
    // "shot_result|123/45/87|543/34/1|198.0.0.126";
    public void OnShotMessageReceived(string message)
    {
        if (!inPlayRoom) return;
        try
        {
            string[] substrings = message.Split('|');
            string[] positions = substrings[1].Split('/');
            Vector3 position = new Vector3(
                float.Parse(positions[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(positions[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(positions[2], CultureInfo.InvariantCulture.NumberFormat));

            string[] rotations = substrings[2].Split('/');
            Quaternion rotation = Quaternion.Euler(
                float.Parse(rotations[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(rotations[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(rotations[2], CultureInfo.InvariantCulture.NumberFormat)
                );
            string ip = substrings[3];

            GameObject objToIgnore;
            if (ip.Equals(ConnectionManager.instance.currentUserData.ip))
            {
                objToIgnore = player;
            }
            else
            {
                PlayerData plData = FindPlayerByIp(ip);
                if (plData != null && plData.controlledGameObject != null)
                {
                    objToIgnore = plData.controlledGameObject;
                }
                else return;
            }
            Action act = Action;
            UnityThread.executeInUpdate(act);
            void Action()
            {
                shootingManager.MakeActualShot(position, rotation, objToIgnore);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("_____EXCEPTION_____");
            Debug.LogError($"EXCEPTION WORKED BECAUSE OF MESSAGE\n{message}");
            Debug.LogError(e.Message + " " + e.StackTrace);
            Debug.LogError("_____EXCEPTION_____");

        }
    }

    public void TryToShootOnline(Vector3 projectileSpawnPoint, Vector3 angleForProjectile)
    {
        string posX = projectileSpawnPoint.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        string posY = projectileSpawnPoint.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        string posZ = projectileSpawnPoint.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

        string rotX = angleForProjectile.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        string rotY = angleForProjectile.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        string rotZ = angleForProjectile.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

        ConnectionManager.instance.SendMessageToServer($"{SHOT_REQUEST}|{posX}/{posY}/{posZ}|" +
            $"{rotX}/{rotY}/{rotZ}", MessageProtocol.TCP);
    }
    #endregion
    #region System
    // basically checks if player that joined room has not been yet added as an GameObject
    void CheckUnspawnedPlayers()
    {
        foreach (PlayerData a in opponents)
        {
            if (a.controlledGameObject == null)
            {
                try
                {
                    a.controlledGameObject = Instantiate(PrefabsHolder.instance.opponent_prefab, a.position, a.rotation);
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
        public PlayerData() { }
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
    void UpdatePlayersPositions()
    {
        if (!inPlayRoom) return;
        if (opponents.Count > 0)
        {
            try
            {
                foreach (PlayerData a in opponents)
                {
                    if (a.controlledGameObject != null)
                    {
                        a.controlledGameObject.transform.position = Vector3.Lerp(a.controlledGameObject.transform.position, a.position, Time.deltaTime * pos_interpolationSpeed);
                        a.controlledGameObject.transform.rotation = Quaternion.Lerp(a.controlledGameObject.transform.rotation, a.rotation, Time.deltaTime * rot_interpolationSpeed);
                    }
                }
            }
            catch (Exception e) { Debug.LogError(e.Message + " " + e.StackTrace); }
        }
    }
    #endregion
    

    
}
