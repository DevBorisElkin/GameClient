using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_MessageFromServer : MonoBehaviour
{
    public TMP_Text messageTxt;

    public void SetUp(string _message)
    {
        messageTxt.text = _message;
    }

    public void OnClick_Close()
    {
        Destroy(gameObject);
    }
}
