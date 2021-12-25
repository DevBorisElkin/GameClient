using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinScreenHolderForStats : MonoBehaviour
{
    public TMP_Text playerNumber;
    public GameObject trophyHolder;
    public GameObject winnerTextHolder;
    public GameObject participantTextHolder;
    public TMP_Text winnerTxt;
    public TMP_Text participantTxt;
    public GameObject fillerObj;
    public TMP_Text stats_kills;
    public TMP_Text stats_deaths;
    public TMP_Text stats_runes;
    public float scaleIfFirst = 0.9f;
    public float scaleIfNotFirst = 0.75f;

    public void SetUp(string number, string nickname, string kills, string deaths, string runes, bool winner)
    {
        playerNumber.text = number;
        trophyHolder.SetActive(winner);
        winnerTextHolder.SetActive(winner);
        participantTextHolder.SetActive(!winner);
        fillerObj.SetActive(!winner);

        if (winner)
        {
            transform.localScale = Vector3.one * scaleIfFirst;
            winnerTxt.text = nickname;
        }
        else
        {
            transform.localScale = Vector3.one * scaleIfNotFirst;
            participantTxt.text = nickname;
        }

        stats_kills.text = kills;
        stats_deaths.text = deaths;
        stats_runes.text = runes;
    }
}
