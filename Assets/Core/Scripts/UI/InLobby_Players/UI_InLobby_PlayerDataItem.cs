using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_InLobby_PlayerDataItem : MonoBehaviour
{
    public TMP_Text nicknameTxt;
    public TMP_Text killsTxt;
    public TMP_Text deathsTxt;

    public void SetUpData(string nickname, int kills, int deaths)
    {
        nicknameTxt.text = nickname;
        killsTxt.text = kills.ToString();
        deathsTxt.text = deaths.ToString();
    }
}
