using BorisUnityDev.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConnectionManager;

public class UI_TestConnectionPanel : MonoBehaviour
{
    public GameObject panelConnect;
    public GameObject panelSendReceive;

    public InputField enterTextInputField; 

    public Text ip_port_connectedTo;
    public Text serverOutputText;


    private void Start()
    {
        if (ConnectionManager.instance.currentStatus.Equals(ClientCurrentStatus.Connected))
        {
            panelConnect.SetActive(false);
            panelSendReceive.SetActive(true);
        }
        else if (ConnectionManager.instance.currentStatus.Equals(ClientCurrentStatus.Disconnected))
        {
            panelConnect.SetActive(true);
            panelSendReceive.SetActive(false);
        }
        
    }

    public void OnClick_Connect()
    {
        Debug.Log("Trying to connect to the server");
        ConnectionManager.instance.Connect();
    }

    public void OnConnectedCallback(string ip_port)
    {
        ip_port_connectedTo.text = ip_port;

        panelConnect.SetActive(false);
        panelSendReceive.SetActive(true);
    }
    public void OnDisconnectedCallback()
    {
        ip_port_connectedTo.text = "";

        panelConnect.SetActive(true);
        panelSendReceive.SetActive(false);
    }

    public void OnClick_ConnectToPlayroom()
    {
        ConnectionManager.instance.OnClick_ConnectToPlayroom();
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

    public void OnMessageReceived(string mes)
    {
        serverOutputText.text += $"\n{mes}";
    }

    public void OnClick_Disconnect()
    {
        panelConnect.SetActive(true);
        panelSendReceive.SetActive(false);

        ConnectionManager.instance.Disconnect();
    }

    string message;
    public void OnEditText(string message)
    {
        this.message = message;
    }
}
