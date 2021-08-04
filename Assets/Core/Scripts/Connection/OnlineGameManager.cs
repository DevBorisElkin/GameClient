using BorisUnityDev.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
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

    GameObject player;
    private void Awake()
    {
        InitSingleton();
        SpawnPlayer();
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

    void SpawnPlayer()
    {
        if (SceneManager.GetActiveScene().name.Equals("NetworkingGameScene"))
            player = Instantiate(PrefabsHolder.instance.player_prefab, spawnPosition, Quaternion.identity);
    }

    public void OnPlayRoomEntered()
    {
        opponents = new List<Player>();
        inPlayRoom = true;
    }

    public void OnPlayRoomExited()
    {
        inPlayRoom = false;
    }

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


    public void OnPlayerDisconnectedFromPlayroom(string message)
    {
        if (!inPlayRoom) return;
        string[] substrings = message.Split('|');
        string leftPlayerIp = substrings[3];

        Player leftPlayer = FindPlayerByIp(leftPlayerIp);
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

            foreach (string a in playersData)
            {
                string[] subdata = a.Split(',');

                Player correctPlayer = FindPlayerByIp(subdata[1]);

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

                    Player newlyCreatedPlayer = new Player();
                    newlyCreatedPlayer.nickname = subdata[0];
                    newlyCreatedPlayer.ip = subdata[1];
                    newlyCreatedPlayer.position = new Vector3(float.Parse(coordinatesXYZ[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(coordinatesXYZ[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(coordinatesXYZ[2], System.Globalization.CultureInfo.InvariantCulture));
                    newlyCreatedPlayer.rotation = Quaternion.Euler(float.Parse(rotation[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(rotation[1], System.Globalization.CultureInfo.InvariantCulture), float.Parse(rotation[2], System.Globalization.CultureInfo.InvariantCulture));

                    opponents.Add(newlyCreatedPlayer);

                    // creating that player
                    //...
                }

            }

            if (opponents.Count > 0)
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

    // basically checks if player that joined room has not been yet added as an GameObject
    void CheckUnspawnedPlayers()
    {
        foreach (Player a in opponents)
        {
            if (a.controlledGameObject == null)
            {
                try
                {
                    a.controlledGameObject = Instantiate(PrefabsHolder.instance.opponent_prefab, a.position, a.rotation);
                }
                catch (Exception e) { Debug.LogError(e.Message + " " + e.StackTrace); }
            }
        }
    }

    void CheckRemoveAndDeleteLeftPlayers()
    {
        try
        {
            List<Player> playersToDelete = new List<Player>();

            foreach (Player a in opponents)
            {
                if (a.controlledGameObject != null && a.playerLeft)
                    playersToDelete.Add(a);
            }

            foreach (Player a in playersToDelete)
            {
                Destroy(a.controlledGameObject);
                opponents.Remove(a);
            }
        }
        catch (Exception e) { Debug.LogError(e.Message + " " + e.StackTrace); }
    }

    List<Player> opponents;

    public class Player
    {
        public string nickname;
        public string ip;

        public bool playerLeft;

        public Vector3 position;
        public Quaternion rotation;

        public GameObject controlledGameObject;
    }

    public Player FindPlayerByIp(string ip)
    {
        foreach (Player a in opponents)
        {
            if (a.ip.Equals(ip)) return a;
        }
        return null;
    }

    private void Update()
    {
        if (!inPlayRoom) return;

        if (opponents.Count > 0)
        {
            try
            {
                foreach (Player a in opponents)
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
}
