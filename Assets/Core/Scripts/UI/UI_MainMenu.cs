using BorisUnityDev.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ConnectionManager;
using static Enums;
using static UI_GlobalManager;

public class UI_MainMenu : MonoBehaviour
{
    #region variables

    [Header("General")]
    public Color colorOkay;
    public Color colorError;
    public Color colorNeutral;
    public TMP_Text connectionStatus;
    string str_connected = "Connected to Server";
    string str_disconnected = "No connection to Server";

    [Space(5f)]
    public GameObject introPanel;
    public GameObject authenticationPanel;
    public GameObject registrationPanel;

    [Header("Authenticate")]
    public TMP_InputField auth_EnterLogin;
    public TMP_InputField auth_EnterPassword;
    public TMP_Text auth_errorResult;

    [Header("Register")]
    public TMP_InputField reg_EnterLogin;
    public TMP_InputField reg_EnterPassword;
    public TMP_InputField reg_EnterNickname;
    public TMP_Text reg_errorResult;


    [Header("Legacy")]
    public GameObject panelConnect;  // panel connect now will be the parent of 3 new panels
    public GameObject panelSendReceive;

    public InputField enterTextInputField;

    #endregion
    private void Start()
    {
        UI_GlobalManager.instance.ManageSceneOnLoad();
    }

    public void MainMenu_Connected(ClientStatus newClientStatus, bool alterAll = true)
    {
        if (newClientStatus.Equals(ClientStatus.Disconnected))
        {
            connectionStatus.text = str_disconnected;
            connectionStatus.color = colorError;

            if (alterAll)
                AlterAll_One(1);
        }
        else if (newClientStatus.Equals(ClientStatus.Connected))
        {
            connectionStatus.text = str_connected;
            connectionStatus.color = colorOkay;

            if (alterAll)
                AlterAll_One(1);
        }
        else if (newClientStatus.Equals(ClientStatus.Authenticated))
        {
            connectionStatus.text = str_connected;
            connectionStatus.color = colorOkay;

            if (alterAll)
                AlterAll_One(2);
        }
    }
    void AlterAll_One(int number)
    {
        if(number == 1)
        {
            panelConnect.SetActive(true);
            panelSendReceive.SetActive(false);

            introPanel.SetActive(true);
            authenticationPanel.SetActive(false);
            registrationPanel.SetActive(false);
        }
        else if(number == 2)
        {
            panelConnect.SetActive(false);
            panelSendReceive.SetActive(true);

            introPanel.SetActive(false);
            authenticationPanel.SetActive(false);
            registrationPanel.SetActive(false);
        }
        
        loginAuth = "";
        passwordAuth = "";

        loginReg = "";
        passwordReg = "";
        nicknameReg = "";

        auth_EnterLogin.SetTextWithoutNotify("");
        auth_EnterPassword.SetTextWithoutNotify("");

        reg_EnterLogin.SetTextWithoutNotify("");
        reg_EnterPassword.SetTextWithoutNotify("");
        reg_EnterNickname.SetTextWithoutNotify("");

        auth_errorResult.text = "";
        reg_errorResult.text = "";
        auth_errorResult.color = colorNeutral;
        reg_errorResult.color =  colorNeutral;
    }
    public void MainMenu_Authenticated(bool state)
    {
        panelConnect.SetActive(!state);
        panelSendReceive.SetActive(state);
    }

    public void OnClick_ChoiceAuthenticate()
    {
        introPanel.SetActive(false);
        authenticationPanel.SetActive(true);
        registrationPanel.SetActive(false);
    }
    public void OnClick_ChoiceRegister()
    {
        introPanel.SetActive(false);
        authenticationPanel.SetActive(false);
        registrationPanel.SetActive(true);
    }
    public void OnClick_BackToChoice()
    {
        introPanel.SetActive(true);
        authenticationPanel.SetActive(false);
        registrationPanel.SetActive(false);
    }

    #region AUTHENTICATE VARIABLES
    string loginAuth;
    string passwordAuth;
    public void OnEdit_AuthLogin(string text)
    {
        loginAuth = text;
    }
    public void OnEdit_AuthPassword(string text)
    {
        passwordAuth = text;
    }

    public void OnClick_TryToAuthenticate()
    {
        // TODO CHECK CONNECTION
        auth_errorResult.color = colorNeutral;

        if (loginAuth.Equals(""))
        {
            auth_errorResult.text = "login can't be empty";
        }
        else if (loginAuth.Length > 10)
        {
            auth_errorResult.text = "login shouldn't be longer than 10 characters";
        }else if(passwordAuth.Length < 5)
        {
            auth_errorResult.text = "password can't be less than 5 characters";
        }
        else
        {
            // TODO perform auth action

            // TODO BLOCK INTERFACE WHILE WAITING FOR RESPONSE
            ConnectionManager.instance.LogIn(loginAuth, passwordAuth);
        }
    }

    #endregion

    #region REGISTER VARIABLES
    string loginReg;
    string passwordReg;
    string nicknameReg;
    public void OnEdit_RegLogin(string text)
    {
        loginReg = text;
    }
    public void OnEdit_RegPassword(string text)
    {
        passwordReg = text;
    }
    public void OnEdit_RegNickname(string text)
    {
        nicknameReg = text;
    }

    public void OnClick_TryToRegister()
    {
        reg_errorResult.color = colorNeutral;
        Debug.Log("Now you can't register account");
    }

    #endregion

    #region newtwork results for authentication and registration

    public void ShowAuthOrRegResult(string result, bool useForAuthentication = true)
    {
        if (useForAuthentication)
        {
            auth_errorResult.text = result;
            auth_errorResult.color = colorError;
        }
        else
        {
            reg_errorResult.text = result;
            reg_errorResult.color = colorError;
        }
    }

    #endregion;

    #region ALREADY CONNECTED
    // TODO Connection now should be automatic
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

    #endregion
}
