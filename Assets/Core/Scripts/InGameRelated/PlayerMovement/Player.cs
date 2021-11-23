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
    public List<RuneVisual> debuffVisuals;
    [HideInInspector] public List<Rune> runeEffects;
    [HideInInspector] public List<Rune> debuffEffects;

    public bool collidedWithSpikeTrap;

    [HideInInspector] public bool localPlayer;

    public void SetUpPlayer(PlayerData _playerData, bool _localPlayer = false)
    {
        localPlayer = _localPlayer;
        playerData = _playerData;
        nicknameCanvas.SetUp(this, playerData);
        ResetAllRuneEffects();
        runeEffects = new List<Rune>();
        debuffEffects = new List<Rune>();
    }

    #region RuneEffects
    public void AddRuneEffect(Rune rune)
    {
        foreach(var a in runeVisuals)
            if (a.runeType == rune) a.gameObject.SetActive(true);

        if (!runeEffects.Contains(rune)) runeEffects.Add(rune);

        if (rune == Rune.LightBlue) CheckLightBlueParticles(true);
        if (rune == Rune.Golden) CheckGoldenParticles(true);
    }
    public void RemoveRuneEffect(Rune rune)
    {
        foreach (var a in runeVisuals)
            if (a.runeType == rune) a.gameObject.SetActive(false);

        if (runeEffects.Contains(rune)) runeEffects.Remove(rune);

        if (rune == Rune.LightBlue) CheckLightBlueParticles(false);
        if (rune == Rune.Golden) CheckGoldenParticles(false);
    }
    public void ResetAllRuneEffects()
    {
        foreach (var a in runeVisuals)
            a.gameObject.SetActive(false);

        runeEffects = new List<Rune>();

        CheckLightBlueParticles(false);
        CheckGoldenParticles(false);
    }
    #endregion

    #region debuffs

    public void AddDebuffEffect(Rune rune)
    {
        foreach (var a in debuffVisuals)
            if (a.runeType == rune) a.gameObject.SetActive(true);

        if (!debuffEffects.Contains(rune)) debuffEffects.Add(rune);

        if (localPlayer)
        {
            VibrationsManager.OnLocalPlayerReceivedDebuff_Vibrations(OnlineGameManager.instance.playerMovementConetroller);
        }
    }
    public void RemoveDebuffEffect(Rune rune)
    {
        foreach (var a in debuffVisuals)
            if (a.runeType == rune) a.gameObject.SetActive(false);

        if (debuffEffects.Contains(rune)) debuffEffects.Remove(rune);

        if (localPlayer && debuffEffects.Count == 0)
            VibrationsManager.OnLocalPlayerDebuffEnded_Vibrations(OnlineGameManager.instance.playerMovementConetroller);
    }
    public void ResetAllDebuffEffects()
    {
        foreach (var a in debuffVisuals)
            a.gameObject.SetActive(false);

        debuffEffects = new List<Rune>();

        if (localPlayer && debuffEffects.Count == 0)
            VibrationsManager.OnLocalPlayerDebuffEnded_Vibrations(OnlineGameManager.instance.playerMovementConetroller);
    }

    #endregion

    public Transform lightBlueAdditionalParticles;
    public Transform parentForLightBlueAdditionalParticles;
    public Transform goldenAdditionalParticles;
    public Transform parentFoGoldenAdditionalParticles;
    public void CheckLightBlueParticles(bool turnedOn)
    {
        if (lightBlueAdditionalParticles.transform == null) return;
        if (turnedOn)
        {
            lightBlueAdditionalParticles.transform.SetParent(null);
            lightBlueAdditionalParticles.gameObject.SetActive(true);
        }
        else
        {
            lightBlueAdditionalParticles.transform.SetParent(parentForLightBlueAdditionalParticles);
            lightBlueAdditionalParticles.gameObject.SetActive(false);
        }
    }
    public void CheckGoldenParticles(bool turnedOn)
    {
        if (goldenAdditionalParticles.transform == null) return;
        if (turnedOn)
        {
            goldenAdditionalParticles.transform.SetParent(null);
            goldenAdditionalParticles.gameObject.SetActive(true);
        }
        else
        {
            goldenAdditionalParticles.transform.SetParent(parentFoGoldenAdditionalParticles);
            goldenAdditionalParticles.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        ResetAllRuneEffects();
    }
}
