using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OnlineGameManager;

public class Player : MonoBehaviour
{
    public NicknameCanvas nicknameCanvas;
    public PlayerData playerData;

    public Transform projectileSpawnPoint;

    public void SetUpPlayer(PlayerData _playerData)
    {
        playerData = _playerData;
        nicknameCanvas.SetUp(this, playerData);
    }
}
