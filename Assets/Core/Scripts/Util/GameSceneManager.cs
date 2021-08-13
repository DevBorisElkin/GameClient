using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField]
    public List<SpawnPosition> spawnPositions = new List<SpawnPosition>();
    private void Start()
    {
        OnlineGameManager.instance.OnPlayRoomEntered();
        OnlineGameManager.instance.SpawnPlayer(spawnPositions);
    }

    private void OnDestroy()
    {
        OnlineGameManager.instance.OnPlayRoomExited();
    }

    [System.Serializable]
    public class SpawnPosition
    {
        public int index;
        public GameObject spawnPos;
    }

    public void OnClick_TryToJump()
    {
        OnlineGameManager.instance.playerMovementConetroller.TryToJump_Request();
    }
}
