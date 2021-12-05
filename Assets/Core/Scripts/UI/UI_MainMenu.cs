using BorisUnityDev.Networking;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ConnectionManager;
using static EnumsAndData;
using static UI_GlobalManager;
using static Util_UI;

public class UI_MainMenu : MonoBehaviour
{
    #region _____variables__________

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

    [Header("Register")]
    public TMP_InputField reg_EnterLogin;
    public TMP_InputField reg_EnterPassword;
    public TMP_InputField reg_EnterNickname;

    [Header("Logged in")]
    public UI_PlayroomsManager ui_PlayroomsManager;
    public GameObject panel_connection_main;
    public GameObject panel_connection_profile;
    public GameObject panel_connection_playrooms;
    [Space(5f)]
    public GameObject rootForServerMessages;

    [Header("Legacy")]
    public GameObject panelConnect;  // panel connect now will be the parent of 3 new panels
    public GameObject panelSendReceive;

    public InputField enterTextInputField;
    #endregion _______________
    private void Start()
    {
        UI_GlobalManager.instance.ManageSceneOnLoad();
    }

    public void MainMenu_Connected(ClientStatus newClientStatus, bool swichBetweenConnectionTypes = true)
    {
        if (newClientStatus.Equals(ClientStatus.Disconnected))
        {
            connectionStatus.text = str_disconnected;
            connectionStatus.color = colorError;
        }
        else if (newClientStatus.Equals(ClientStatus.Connected) || newClientStatus.Equals(ClientStatus.Authenticated))
        {
            connectionStatus.text = str_connected;
            connectionStatus.color = colorOkay;
        }
        if (swichBetweenConnectionTypes)
            DetailedAdjustment(newClientStatus);
    }
    void DetailedAdjustment(ClientStatus status)
    {
        if(status.Equals(ClientStatus.Disconnected) || status.Equals(ClientStatus.Connected))
        {
            panelConnect.SetActive(true);
            panelSendReceive.SetActive(false);

            introPanel.SetActive(true);
            Animation_OnOpen(Auth_Window.Login);
            //authenticationPanel.SetActive(false);
            //registrationPanel.SetActive(false);

            string storedLogin = PlayerPrefs.GetString(CODE_SAVED_LOGIN, "");
            string storedpassword = PlayerPrefs.GetString(CODE_SAVED_PASSWORD, "");
            if(!string.IsNullOrEmpty(storedLogin) && !string.IsNullOrEmpty(storedpassword))
            {
                loginAuth = storedLogin;
                auth_EnterLogin.SetTextWithoutNotify(storedLogin);
                passwordAuth = storedpassword;
                auth_EnterPassword.SetTextWithoutNotify(storedpassword);
            }
            else
            {
                loginAuth = "";
                passwordAuth = "";
                auth_EnterLogin.SetTextWithoutNotify("");
                auth_EnterPassword.SetTextWithoutNotify("");
            }
        }
        else if(status.Equals(ClientStatus.Authenticated))
        {
            panelConnect.SetActive(false);
            panelSendReceive.SetActive(true);

            introPanel.SetActive(false);
            //authenticationPanel.SetActive(false);
            //registrationPanel.SetActive(false);

            panel_connection_profile.SetActive(false);
            panel_connection_playrooms.SetActive(false);
        }
        
        loginReg = "";
        passwordReg = "";
        nicknameReg = "";

        reg_EnterLogin.SetTextWithoutNotify("");
        reg_EnterPassword.SetTextWithoutNotify("");
        reg_EnterNickname.SetTextWithoutNotify("");

        settingsPanel.SetActive(false);
    }

    public void OnClick_ChoiceAuthenticate() { Manage_IntroAuthRegister_Panels(false, true, false); }
    public void OnClick_ChoiceRegister() { Manage_IntroAuthRegister_Panels(false, false, true); }
    public void OnClick_BackToChoice() { Manage_IntroAuthRegister_Panels(true, false, false); }
    void Manage_IntroAuthRegister_Panels(bool intro, bool auth, bool register)
    {
        introPanel.SetActive(intro);
        //authenticationPanel.SetActive(auth);
        //registrationPanel.SetActive(register);
    }

    #region LogIn Register panels switching

    public float modalWindowOpenTime = 0.25f;
    public float modalWindowCloseTime = 0.25f;
    public Ease modalWindowScaleInEase = Ease.Linear;
    public Ease modalWindowScaleOutEase = Ease.Linear;

    Tween authWindowScaleInTween;
    Tween authWindowScaleOutTween;
    Tween regWindowScaleInTween;
    Tween regWindowScaleOutTween;
    void Animation_OnOpen(Auth_Window authWindow)
    {
        ResetAllTweens();
        GameObject panel = authWindow == Auth_Window.Login ? authenticationPanel : registrationPanel;
        GameObject oppositePanel = authWindow == Auth_Window.Login ? registrationPanel : authenticationPanel;

        panel.gameObject.SetActive(true);
        oppositePanel.gameObject.SetActive(false);

        panel.transform.localScale = Vector3.zero;
        if(authWindow == Auth_Window.Login)
            authWindowScaleInTween = panel.transform.DOScale(Vector3.one, modalWindowOpenTime).SetEase(modalWindowScaleInEase);
        else
            regWindowScaleInTween = panel.transform.DOScale(Vector3.one, modalWindowOpenTime).SetEase(modalWindowScaleInEase);
    }

    void Animation_OnClose(Auth_Window authWindow)
    {
        ResetAllTweens();
        GameObject panel = authWindow == Auth_Window.Login ? authenticationPanel : registrationPanel;
        panel.transform.localScale = Vector3.one;
        if (authWindow == Auth_Window.Login)
            authWindowScaleOutTween = panel.transform.DOScale(Vector3.zero, modalWindowCloseTime).SetEase(modalWindowScaleOutEase);
        else
            regWindowScaleInTween = panel.transform.DOScale(Vector3.zero, modalWindowCloseTime).SetEase(modalWindowScaleOutEase);
    }
    void ResetAllTweens()
    {
        if (authWindowScaleInTween != null && authWindowScaleInTween.IsPlaying())
        {
            authWindowScaleInTween.Complete();
            authWindowScaleInTween = null;
        }
        if (authWindowScaleOutTween != null && authWindowScaleOutTween.IsPlaying())
        {
            authWindowScaleOutTween.Complete();
            authWindowScaleOutTween = null;
        }
        if (regWindowScaleInTween != null && regWindowScaleInTween.IsPlaying())
        {
            regWindowScaleInTween.Complete();
            regWindowScaleInTween = null;
        }
        if (regWindowScaleOutTween != null && regWindowScaleOutTween.IsPlaying())
        {
            regWindowScaleOutTween.Complete();
            regWindowScaleOutTween = null;
        }
    }

    public void OnClick_SwitchToLogIn() => Animation_OnOpen(Auth_Window.Login);
    public void OnClick_SwitchToRegister() => Animation_OnOpen(Auth_Window.Register);
    public void OnClick_ForgotPassword() => UI_GlobalManager.Message_ModalWindow("Unfortunately, restore password function is not currently supported", MessageFromServer_MessageType.Info);

    #endregion

    #region AUTHENTICATE VARIABLES
    string loginAuth;
    string passwordAuth;
    public void OnEdit_AuthLogin(string text) { loginAuth = text; }
    public void OnEdit_AuthPassword(string text) { passwordAuth = text; }

    public void OnClick_TryToAuthenticate()
    {
        if (!UI_GlobalManager.instance.recordedStatus.Equals(ClientStatus.Connected))
        {
            UI_GlobalManager.Message("Connection to the internet required!", MessageFromServer_WindowType.LightWindow, MessageFromServer_MessageType.Error);
            return;
        }

        if (   IsStringClearFromErrors   (loginAuth,    null, Input_Field.Login)
            && IsStringClearFromErrors   (passwordAuth, null, Input_Field.Password))
        {
            // TODO BLOCK INTERFACE WHILE WAITING FOR RESPONSE
            ConnectionManager.instance.LogIn(loginAuth, passwordAuth);
        }
    }
    #endregion

    #region _____REGISTER VARIABLES__________
    string loginReg;
    string passwordReg;
    string nicknameReg;
    public void OnEdit_RegLogin(string text) { loginReg = text; }
    public void OnEdit_RegPassword(string text) { passwordReg = text; }
    public void OnEdit_RegNickname(string text) { nicknameReg = text; }

    public void OnClick_TryToRegister()
    {
        if (!UI_GlobalManager.instance.recordedStatus.Equals(ClientStatus.Connected))
        {
            UI_GlobalManager.Message("Connection to the internet required!", MessageFromServer_WindowType.LightWindow, MessageFromServer_MessageType.Error);
            return;
        }

        if (   IsStringClearFromErrors(loginReg, null,    Input_Field.Login)
            && IsStringClearFromErrors(passwordReg, null, Input_Field.Password)
            && IsStringClearFromErrors(nicknameReg, null, Input_Field.Nickname))
        {
            // TODO BLOCK INTERFACE WHILE WAITING FOR RESPONSE
            ConnectionManager.instance.Register(loginReg, passwordReg, nicknameReg);
        }
    }
    #endregion ___________________________

    #region _____newtwork results for authentication and registration_____

    public void ShowAuthOrRegResult(string result)
    {
        UI_GlobalManager.Message(result, MessageFromServer_WindowType.LightWindow, MessageFromServer_MessageType.Error);
    }
    #endregion ___________________________________________________________

    #region _____ALREADY CONNECTED_____________
    public void OnClick_LogOut() { ConnectionManager.instance.Disconnect(false); }
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
    public void OnEditText(string message) { this.message = message; }

    public void OnClick_OpenPlayroomsList()
    {
        ui_PlayroomsManager.ClearItemsHolder();
        panel_connection_playrooms.SetActive(true);
        ConnectionManager.instance.RequestListOfPlayrooms();
    }
    public void OnClick_ClosePlayroomsList() { panel_connection_playrooms.SetActive(false); }
    public void OnClick_RefreshPlayroomsList() { ConnectionManager.instance.RequestListOfPlayrooms(); }

    public void OnClick_CreatePlayroom()
    {
        GameObject newObj = Instantiate(PrefabsHolder.instance.ui_createLobby_prefab, rootForServerMessages.transform);
    }
    #endregion ________________________________

    #region ______SETTINGS_PANEL______
    [Header("Settings")]
    public GameObject settingsPanel;
    public UI_Settings ui_settings;

    public void OnClick_OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
        ui_settings.OnPanelOpened();
    }
    public void OnClick_CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }

    #endregion
}
