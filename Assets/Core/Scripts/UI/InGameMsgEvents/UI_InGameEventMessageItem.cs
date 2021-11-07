using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EnumsAndData;

public class UI_InGameEventMessageItem : MonoBehaviour
{
    [Header("General")]
    public CanvasGroup coreCanvasGroup;
    public float staysFullyVisible = 5f;
    public float disappears = 2f;
    float eachIterationBlurTime = 0.025f;

    [Header("KillEvent")]
    public GameObject panelKillEvent;

    public TMP_Text killerTxt;
    public TMP_Text victimTxt;

    public Image weaponImage;
    public Image spikesImage;

    [Header("SuicideEvent")]
    public GameObject panelSuicideEvent;

    public TMP_Text victimSuicideTxt;

    public Image suicideImage;
    public Image spikesSuicideImage;

    [Header("Enter Exit Event")]
    public GameObject panelEnterExit;

    public TMP_Text enterExitNickname;

    public Image enterImg;
    public Image exitImg;

    [Header("Pick Up Rune")]
    public GameObject panelPickUpRune;

    public TMP_Text runePickerTxt;
    public Image runeImage;
    public void SetUpDeathMessage(MessageType messageType, DeathDetails deathDetails, ReasonOfDeath reasonOfDeath, string killerNick, string victimNick)
    {
        DefaultPanel();

        if (messageType.Equals(MessageType.Kill))
        {
            panelKillEvent.SetActive(true);
            killerTxt.text = killerNick;
            victimTxt.text = victimNick;
            if (deathDetails.Equals(DeathDetails.FellOutOfMap))
            {
                weaponImage.gameObject.SetActive(true);
                spikesImage.gameObject.SetActive(false);
            }
            else if (deathDetails.Equals(DeathDetails.TouchedSpikes))
            {
                weaponImage.gameObject.SetActive(true);
                spikesImage.gameObject.SetActive(true);
            }
        }else if (messageType.Equals(MessageType.Suicide))
        {
            panelSuicideEvent.SetActive(true);
            victimSuicideTxt.text = $"{victimNick} Made a wrong choice";
            if (deathDetails.Equals(DeathDetails.FellOutOfMap))
            {
                suicideImage.gameObject.SetActive(true);
                spikesSuicideImage.gameObject.SetActive(false);
            }
            else if (deathDetails.Equals(DeathDetails.TouchedSpikes))
            {
                suicideImage.gameObject.SetActive(true);
                spikesSuicideImage.gameObject.SetActive(true);
            }
        }
        StartCoroutine(DestroyCoroutine());
    }

    public void OnEnterExitMessage(MessageType messageType, string nicnameOfPlayer)
    {
        DefaultPanel();

        panelEnterExit.SetActive(true);
        if (messageType.Equals(MessageType.Enter))
        {
            enterExitNickname.text = $"{nicnameOfPlayer} Joined playroom";
            enterImg.gameObject.SetActive(true);
            exitImg.gameObject.SetActive(false);
        }
        else
        {
            enterExitNickname.text = $"{nicnameOfPlayer} left Playroom";
            enterImg.gameObject.SetActive(false);
            exitImg.gameObject.SetActive(true);
        }
        StartCoroutine(DestroyCoroutine());
    }

    public void OnRunePickupMessage(Rune runeType, string nicknameOfPicker)
    {
        DefaultPanel();

        panelPickUpRune.SetActive(true);
        runePickerTxt.text = nicknameOfPicker;
        Sprite foundSprite = PrefabsHolder.instance.GetSpriteByRuneType(runeType);
        if (foundSprite != null) runeImage.sprite = foundSprite;

        StartCoroutine(DestroyCoroutine());
    }

    void DefaultPanel()
    {
        panelKillEvent.SetActive(false);
        panelSuicideEvent.SetActive(false);
        panelEnterExit.SetActive(false);
        weaponImage.gameObject.SetActive(false);
        spikesImage.gameObject.SetActive(false);
        suicideImage.gameObject.SetActive(false);
        spikesSuicideImage.gameObject.SetActive(false);
        enterImg.gameObject.SetActive(false);
        exitImg.gameObject.SetActive(false);
        panelPickUpRune.SetActive(false);
    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(staysFullyVisible);

        float blurEachIterations = disappears / eachIterationBlurTime;
        float eachIterationTakeaway = 1 / blurEachIterations;
        //Debug.Log("blurEachIteration: "+ blurEachIterations);
        while (coreCanvasGroup.alpha > 0)
        {
            yield return new WaitForSeconds(eachIterationBlurTime);
            coreCanvasGroup.alpha -= eachIterationTakeaway;
            //Debug.Log(coreCanvasGroup.alpha);
        }
        Destroy(gameObject);
    }
}
