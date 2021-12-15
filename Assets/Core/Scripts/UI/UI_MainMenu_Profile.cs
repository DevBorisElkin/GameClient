using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static DataTypes;
using TMPro;
using UnityEngine.UI;
using static Util_UI;
using static NetworkingMessageAttributes;

public class UI_MainMenu_Profile : MonoBehaviour
{
    public Image backgroundPanel;
    public GameObject profileModalWindow;
    

    [Header("User Data")]
    public TMP_Text nickname;
    public TMP_Text dbId;
    public GameObject userAccessLevel_User;
    public GameObject userAccessLevel_Admin;
    public GameObject userAccessLevel_Admin_Particles;
    public TMP_Text userAccessLevel_User_Txt;
    public TMP_Text userAccessLevel_Admin_Txt;
    public TMP_Text totalGames;
    public TMP_Text totalVictories;
    public TMP_Text kills;
    public TMP_Text deaths;
    public TMP_Text runes_picked_up;


    [Header("Tween Settings")]
    public Color transparentColor;
    public Color normalColor;

    public float modalWindowOpenTime = 0.25f;
    public float modalWindowCloseTime = 0.25f;
    public Ease modalWindowScaleInEase = Ease.Linear;
    public Ease modalWindowScaleOutEase = Ease.Linear;

    Tween profileWindowScaleInTween;
    Tween profileWindowScaleOutTween;

    Tween backgroundWindowFadeInTween;
    Tween backgroundWindowFadeOutTween;

    public void OnClick_OpenProfile()
    {
        Animation_OnOpen();
        UpdateProfilePanelValues(ConnectionManager.instance.currentUserData);
    }

    public void OnClick_CloseProfile()
    {
        Animation_OnClose();
    }

    void Animation_OnOpen()
    {
        ResetAllTweens();

        profileModalWindow.transform.localScale = Vector3.zero;
        profileWindowScaleInTween = profileModalWindow.transform.DOScale(Vector3.one, modalWindowOpenTime).SetEase(modalWindowScaleInEase);

        backgroundPanel.gameObject.SetActive(true);
        backgroundPanel.color = transparentColor;
        backgroundWindowFadeInTween = backgroundPanel.DOColor(normalColor, modalWindowOpenTime).SetEase(modalWindowScaleInEase);
    }

    void Animation_OnClose()
    {
        ResetAllTweens();

        profileModalWindow.transform.localScale = Vector3.one;
        profileWindowScaleOutTween = profileModalWindow.transform.DOScale(Vector3.zero, modalWindowCloseTime).SetEase(modalWindowScaleInEase);

        backgroundPanel.color = normalColor;
        backgroundWindowFadeOutTween = backgroundPanel.DOColor(transparentColor, modalWindowCloseTime + 0.1f).SetEase(modalWindowScaleInEase).OnComplete(() => {
            backgroundPanel.gameObject.SetActive(false);
        });
    }
    void ResetAllTweens()
    {
        if (profileWindowScaleInTween != null)
        {
            profileWindowScaleInTween.Complete();
            profileWindowScaleInTween = null;
        }
        if (profileWindowScaleOutTween != null)
        {
            profileWindowScaleOutTween.Complete();
            profileWindowScaleOutTween = null;
        }
        if (backgroundWindowFadeInTween != null)
        {
            backgroundWindowFadeInTween.Complete();
            backgroundWindowFadeInTween = null;
        }
        if (backgroundWindowFadeOutTween != null)
        {
            backgroundWindowFadeOutTween.Complete();
            backgroundWindowFadeOutTween = null;
        }
    }

    public void UpdateProfilePanelValues(UserData userData)
    {
        nickname.text = userData.nickname;
        dbId.text = userData.db_id.ToString();
        userAccessLevel_User.SetActive(userData.accessRights == EnumsAndData.AccessRights.User);
        userAccessLevel_Admin.SetActive(userData.accessRights == EnumsAndData.AccessRights.Admin || userData.accessRights == EnumsAndData.AccessRights.SuperAdmin);
        totalGames.text = userData.total_games.ToString();
        totalVictories.text = userData.total_victories.ToString();
        kills.text = userData.kills.ToString();
        deaths.text = userData.deaths.ToString();
        runes_picked_up.text = userData.runes_picked_up.ToString();

        userAccessLevel_User_Txt.text = userData.accessRights.ToString();
        userAccessLevel_Admin_Txt.text = userData.accessRights.ToString();
        userAccessLevel_Admin_Particles.SetActive(userData.accessRights == EnumsAndData.AccessRights.Admin || userData.accessRights == EnumsAndData.AccessRights.SuperAdmin);

    }

    #region Change nickname related

    [Header("Change Nick Panel")]
    public Image changeNickBackgroundPanel;
    public GameObject changeNickModalPanel;
    public Color changeNickBackgroundNormalColor;
    public float changeNickOpenCloseTime = 0.25f;

    public TMP_InputField changeNickInputField;
    string proposedNewNick;

    public void OnClick_OpenChangeNickPanel()
    {
        ChangeNickPanel_OnOpen();
    }
    public void OnClick_CloseChangeNickPanel()
    {
        ChangeNickPanel_OnClose();
    }

    Tween changeNicknameModal_ScaleInTween;
    Tween changeNicknameModal_ScaleOutTween;
    Tween changeNicknameBack_FadeInTween;
    Tween changeNicknameBack_FadeOutTween;
    void ChangeNickPanel_OnOpen()
    {
        ResetAllChangeNicknameTweens();
        ResetChangeNickValues_OnOpen();
        changeNickBackgroundPanel.gameObject.SetActive(true);
        changeNickBackgroundPanel.color = new Color(changeNickBackgroundNormalColor.r, changeNickBackgroundNormalColor.g, changeNickBackgroundNormalColor.b, 0f);
        changeNickModalPanel.transform.localScale = Vector3.zero;

        changeNicknameModal_ScaleInTween = changeNickModalPanel.transform.DOScale(Vector3.one, changeNickOpenCloseTime).SetEase(modalWindowScaleInEase);
        changeNicknameBack_FadeInTween = changeNickBackgroundPanel.DOColor(changeNickBackgroundNormalColor, changeNickOpenCloseTime);
    }
    void ChangeNickPanel_OnClose()
    {
        ResetAllChangeNicknameTweens();
        changeNickBackgroundPanel.color = changeNickBackgroundNormalColor;
        changeNickModalPanel.transform.localScale = Vector3.one;

        changeNicknameModal_ScaleOutTween = changeNickModalPanel.transform.DOScale(Vector3.zero, changeNickOpenCloseTime).SetEase(modalWindowScaleOutEase);
        changeNicknameBack_FadeOutTween = changeNickBackgroundPanel.DOColor(new Color(changeNickBackgroundNormalColor.r, changeNickBackgroundNormalColor.g, changeNickBackgroundNormalColor.b, 0f), changeNickOpenCloseTime).OnComplete(() =>
        {
            changeNickBackgroundPanel.gameObject.SetActive(false);
        });
    }
    void ResetAllChangeNicknameTweens()
    {
        DOTween.Kill(changeNicknameModal_ScaleInTween);
        DOTween.Kill(changeNicknameModal_ScaleOutTween);
        DOTween.Kill(changeNicknameBack_FadeInTween);
        DOTween.Kill(changeNicknameBack_FadeOutTween);
    }
    void ResetChangeNickValues_OnOpen()
    {
        string currentNick = ConnectionManager.instance.currentUserData.nickname;
        changeNickInputField.SetTextWithoutNotify(currentNick);
        proposedNewNick = currentNick;
    }
    public void OnInputField_ProposedNicknameChanged(string newNick)
    {
        proposedNewNick = newNick;
    }
    public void OnClick_SubmitNewNickForChange()
    {
        if (IsStringClearFromErrors(proposedNewNick, null, Input_Field.Nickname, 5, 12))
        {
            ConnectionManager.instance.SendMessageToServer($"{PROMOCODE_FROM_CLIENT}|{SUBCODE_CHANGE_NICKNAME}|{proposedNewNick}");
            ChangeNickPanel_OnClose();
        }
    }

    #endregion
}
