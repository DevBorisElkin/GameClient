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

    public Transform lightBlueAdditionalParticles;
    public Transform parentForLightBlueAdditionalParticles;
    public Transform goldenAdditionalParticles;
    public Transform parentFoGoldenAdditionalParticles;
    public void CheckLightBlueParticles(bool turnedOn)
    {
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
