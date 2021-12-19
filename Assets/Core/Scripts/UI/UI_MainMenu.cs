using BorisUnityDev.Networking;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ConnectionManager;
using static DataTypes;
using static EnumsAndData;
using static UI_GlobalManager;
using static Util_UI;

public class UI_MainMenu : MonoBehaviour
{
    #region _____variables__________

    [Header("General")]
    public UI_MainMenu_Profile ui_profile;

    [Space(5f)]
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
    public GameObject panel_connection_playrooms;
    public RectTransform playroomsRectTransform;

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

            panel_connection_playrooms.SetActive(false);
            panelGameInfo_obj.SetActive(false);
            settingsBackgroundPanel.gameObject.SetActive(false);
            ui_profile.backgroundPanel.gameObject.SetActive(false);
            ui_settings.promocodeBackgroundPanel.gameObject.SetActive(false);

            UpdateProfilePanelValues(ConnectionManager.instance.currentUserData);
        }
        
        loginReg = "";
        passwordReg = "";
        nicknameReg = "";

        reg_EnterLogin.SetTextWithoutNotify("");
        reg_EnterPassword.SetTextWithoutNotify("");
        reg_EnterNickname.SetTextWithoutNotify("");

        settingsBackgroundPanel.gameObject.SetActive(false);
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
        if (authWindowScaleInTween != null)
        {
            authWindowScaleInTween.Complete();
            authWindowScaleInTween = null;
        }
        if (authWindowScaleOutTween != null)
        {
            authWindowScaleOutTween.Complete();
            authWindowScaleOutTween = null;
        }
        if (regWindowScaleInTween != null)
        {
            regWindowScaleInTween.Complete();
            regWindowScaleInTween = null;
        }
        if (regWindowScaleOutTween != null)
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
        ConnectionManager.instance.RequestListOfPlayrooms();

        panel_connection_playrooms.SetActive(true);
        playroomsRectTransform.anchoredPosition = new Vector2(panelGameInfo.rect.width, 0);
        DOTween.To(() => playroomsRectTransform.anchoredPosition, x => playroomsRectTransform.anchoredPosition = x, new Vector2(0f, 0f), fullScreenPanelOpenCloseTime);
    }
    public void OnClick_ClosePlayroomsList() 
    {
        playroomsRectTransform.anchoredPosition = new Vector2(0, 0);
        DOTween.To(() => playroomsRectTransform.anchoredPosition, x => playroomsRectTransform.anchoredPosition = x, new Vector2(playroomsRectTransform.rect.width, 0f), fullScreenPanelOpenCloseTime);
    }
    public void OnClick_RefreshPlayroomsList() { ConnectionManager.instance.RequestListOfPlayrooms(); }

    public void OnClick_CreatePlayroom()
    {
        GameObject newObj = Instantiate(PrefabsHolder.instance.ui_createLobby_prefab, rootForServerMessages.transform);
    }
    #endregion ________________________________

    #region ______SETTINGS_PANEL______
    [Header("Settings")]
    public Image settingsBackgroundPanelHolder;
    public Image settingsBackgroundPanel;
    public GameObject settingsModalPanel;
    public UI_Settings ui_settings;
    public Color settingsNormalColor;

    public void OnClick_OpenSettingsPanel()
    {
        Settings_OnOpen();
        ui_settings.OnPanelOpened();
    }
    public void OnClick_CloseSettingsPanel()
    {
        Settings_OnClose();
    }

    Tween settingsModal_ScaleInTween;
    Tween settingsModal_ScaleOutTween;
    Tween settingsBack_FadeInTween;
    Tween settingsBack_FadeOutTween;
    void Settings_OnOpen()
    {
        ResetAllSettingsTweens();
        settingsBackgroundPanelHolder.gameObject.SetActive(true);
        settingsBackgroundPanel.gameObject.SetActive(true);
        settingsBackgroundPanel.color = new Color(settingsBackgroundPanel.color.r, settingsBackgroundPanel.color.g, settingsBackgroundPanel.color.b, 0f);
        settingsModalPanel.transform.localScale = Vector3.zero;

        settingsModal_ScaleInTween = settingsModalPanel.transform.DOScale(Vector3.one, modalWindowOpenTime).SetEase(modalWindowScaleInEase);
        settingsBack_FadeInTween = settingsBackgroundPanel.DOColor(settingsNormalColor, modalWindowOpenTime);
    }

    void Settings_OnClose()
    {
        ResetAllSettingsTweens();
        settingsBackgroundPanel.color = settingsNormalColor;
        settingsModalPanel.transform.localScale = Vector3.one;

        settingsModal_ScaleOutTween = settingsModalPanel.transform.DOScale(Vector3.zero, modalWindowCloseTime).SetEase(modalWindowScaleOutEase);
        settingsBack_FadeOutTween = settingsBackgroundPanel.DOColor(new Color(settingsNormalColor.r, settingsNormalColor.g, settingsNormalColor.b, 0f), modalWindowCloseTime).OnComplete(() => 
        {
            settingsBackgroundPanelHolder.gameObject.SetActive(false);
            settingsBackgroundPanel.gameObject.SetActive(false);
        });
    }
    void ResetAllSettingsTweens()
    {
        DOTween.Kill(settingsModal_ScaleInTween);
        DOTween.Kill(settingsModal_ScaleOutTween);
        DOTween.Kill(settingsBack_FadeInTween);
        DOTween.Kill(settingsBack_FadeOutTween);
    }
    #endregion

    #region Adaptive Data

    [Header("Adaptive Data")]
    public GameObject profileBtn_User;
    public GameObject profileBtn_Admin;

    public TMP_Text profileBtn_User_nickname;
    public TMP_Text profileBtn_Admin_nickname;

    public void UpdateProfilePanelValues(UserData userData)
    {
        profileBtn_User_nickname.text = userData.nickname;
        profileBtn_Admin_nickname.text = userData.nickname;

        profileBtn_User.SetActive(userData.accessRights == EnumsAndData.AccessRights.User);
        profileBtn_Admin.SetActive(userData.accessRights == EnumsAndData.AccessRights.Admin || userData.accessRights == EnumsAndData.AccessRights.SuperAdmin);
    }

    #endregion


    #region GameHintsAndInformation

    public RectTransform panelGameInfo;
    public GameObject panelGameInfo_obj;
    public float fullScreenPanelOpenCloseTime = 0.35f;

    public void OnClick_OpenGameInfo()
    {
        panelGameInfo.gameObject.SetActive(true);
        panelGameInfo.anchoredPosition = new Vector2(-panelGameInfo.rect.width, 0);
        DOTween.To(() => panelGameInfo.anchoredPosition, x => panelGameInfo.anchoredPosition = x, new Vector2(0f, 0f), fullScreenPanelOpenCloseTime);
    }

    public void OnClick_CloseGameInfo()
    {
        panelGameInfo.anchoredPosition = new Vector2(0, 0);
        DOTween.To(() => panelGameInfo.anchoredPosition, x => panelGameInfo.anchoredPosition = x, new Vector2(-panelGameInfo.rect.width, 0f), fullScreenPanelOpenCloseTime);
    }


    #endregion
}
