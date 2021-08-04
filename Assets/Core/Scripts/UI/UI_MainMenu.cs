using BorisUnityDev.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConnectionManager;

public class UI_MainMenu : MonoBehaviour
{
    public GameObject panelConnect;
    public GameObject panelSendReceive;

    public InputField enterTextInputField;

    private void Start()
    {
        UI_GlobalManager.instance.ManageSceneOnLoad();
    }
    public void MainMenu_Connected(bool state)
    {
        panelConnect.SetActive(!state);
        panelSendReceive.SetActive(state);
    }
    public void OnClick_Connect()
    {
        Debug.Log("Trying to connect to the server");
        ConnectionManager.instance.Connect();
    }
    public void OnClick_Disconnect()
    {
        ConnectionManager.instance.Disconnect();
    }
    public void OnClick_ConnectToPlayroom()
    {
        ConnectionManager.instance.ConnectToPlayroom();
    }
    public void OnClick_SendMessage()
    {
        if (message != "")
        {
            if (message.StartsWith("tcp "))
            {
                message = message.Replace("tcp ", "");
                ConnectionManager.instance.SendMessageToServer(message, MessageProtocol.TCP);
            }
            else if (message.StartsWith("udp "))
            {
                message = message.Replace("udp ", "");
                ConnectionManager.instance.SendMessageToServer(message, MessageProtocol.UDP);
            }
            else
            {
                ConnectionManager.instance.SendMessageToServer(message, MessageProtocol.TCP);
            }
        }
        enterTextInputField.SetTextWithoutNotify("");
    }

    string message;
    public void OnEditText(string message)
    {
        this.message = message;
    }
}
