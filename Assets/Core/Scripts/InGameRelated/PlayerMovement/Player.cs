using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OnlineGameManager;
using static DataTypes;
using static EnumsAndData;

public class Player : MonoBehaviour
{
    public NicknameCanvas nicknameCanvas;
    public PlayerData playerData;

    public Transform projectileSpawnPoint;

    public List<RuneVisual> runeVisuals;

    public bool collidedWithSpikeTrap;

    public void SetUpPlayer(PlayerData _playerData)
    {
        playerData = _playerData;
        nicknameCanvas.SetUp(this, playerData);
        ResetAllRuneEffects();
    }


    public void AddRuneEffect(Rune rune)
    {
        foreach(var a in runeVisuals)
            if (a.runeType == rune) a.gameObject.SetActive(true);
    }
    public void RemoveRuneEffect(Rune rune)
    {
        foreach (var a in runeVisuals)
            if (a.runeType == rune) a.gameObject.SetActive(false);
    }
    public void ResetAllRuneEffects()
    {
        foreach (var a in runeVisuals)
            a.gameObject.SetActive(false);
    }



}
