using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EnumsAndData;

public class UI_MessageFromServer_Light : MonoBehaviour
{
    public Image backgroundImage;

    [Space(5f)]
    public Color simpleColor;
    public Color warningColor;
    public Color errorColor;

    MessageFromServer_MessageType messageType;
    string message;

    public void SetUp(string message, MessageFromServer_MessageType messageType)
    {
        this.messageType = messageType;
        this.message = message;
        SetUpCorrectStyles();
    }

    void SetUpCorrectStyles()
    {
        if (messageType == MessageFromServer_MessageType.Info)
        {
            backgroundImage.color = simpleColor;

        }
        else if (messageType == MessageFromServer_MessageType.Warning)
        {
            backgroundImage.color = warningColor;

        }
        else if (messageType == MessageFromServer_MessageType.Error)
        {
            backgroundImage.color = errorColor;

        }
    }
}
