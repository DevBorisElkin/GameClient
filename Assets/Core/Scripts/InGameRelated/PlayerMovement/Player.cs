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
    [HideInInspector] public List<Rune> runeEffects;

    public bool collidedWithSpikeTrap;

    public void SetUpPlayer(PlayerData _playerData)
    {
        playerData = _playerData;
        nicknameCanvas.SetUp(this, playerData);
        ResetAllRuneEffects();
        runeEffects = new List<Rune>();
    }


    public void AddRuneEffect(Rune rune)
    {
        foreach(var a in runeVisuals)
            if (a.runeType == rune) a.gameObject.SetActive(true);

        if (!runeEffects.Contains(rune)) runeEffects.Add(rune);
    }
    public void RemoveRuneEffect(Rune rune)
    {
        foreach (var a in runeVisuals)
            if (a.runeType == rune) a.gameObject.SetActive(false);

        if (runeEffects.Contains(rune)) runeEffects.Remove(rune);
    }
    public void ResetAllRuneEffects()
    {
        foreach (var a in runeVisuals)
            a.gameObject.SetActive(false);

        runeEffects = new List<Rune>();
    }
}
