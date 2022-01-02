using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static EnumsAndData;
using static Util_UI;
using static NetworkingMessageAttributes;
using UnityEngine.UI;
using DG.Tweening;

public class UI_Settings : MonoBehaviour
{
    #region Main
    // 0 = high, 1 = medium, 2 = low

    public TMP_Dropdown dropdownGraphics;
    public Toggle toggleVibrations;
    public SwitchToggle switchToggleVibrations;

    int currentGraphicsValue;

    public void OnPanelOpened()
    {
        currentGraphicsValue = PlayerPrefs.GetInt(CODE_GRAPHICS_SETTINGS, 0);
        dropdownGraphics.SetValueWithoutNotify(currentGraphicsValue);

        VibrationsManager.hapticsEnabled = PlayerPrefs.GetInt(CODE_VIBRATIONS_SETTINGS, 1) > 0 ? true : false;
        toggleVibrations.SetIsOnWithoutNotify(VibrationsManager.hapticsEnabled);
        switchToggleVibrations.OnSwitchInstant(VibrationsManager.hapticsEnabled);
    }
    public void OnGraphicsChanged(int newGraphicsValue)
    {
        //Debug.Log($"currentGraphicsValue: {currentGraphicsValue} | newGraphicsValue: {newGraphicsValue}");
        if(currentGraphicsValue != newGraphicsValue)
        {
            //Debug.Log("OnGraphicsChanged "+ newGraphicsValue + " ConvertChosenGraphicsToCorrectInt(newGraphicsValue) "+ ConvertChosenGraphicsToCorrectInt(newGraphicsValue));
            Application.targetFrameRate = 60;
            currentGraphicsValue = newGraphicsValue;
            PlayerPrefs.SetInt(CODE_GRAPHICS_SETTINGS, newGraphicsValue);
            QualitySettings.SetQualityLevel(ConvertChosenGraphicsToCorrectInt(newGraphicsValue));
        }
    }

    public void OnVibrationsChanged(bool newValue)
    {
        VibrationsManager.hapticsEnabled = newValue;
        PlayerPrefs.SetInt(CODE_VIBRATIONS_SETTINGS, VibrationsManager.hapticsEnabled ? 1 : 0);
    }

    public static int ConvertChosenGraphicsToCorrectInt(int chosenGraphics)
    {
        switch (chosenGraphics)
        {
            case 0: return 4;
            case 1: return 2;
            case 2: return 0;
            default: return 4;
        }
    }
    #endregion

    #region Promocode

    [Header("Settings")]
    public Image promocodeBackgroundPanelHolder;
    public Image promocodeBackgroundPanel;
    public GameObject promocodeModalPanel;
    public Color promocodeBackgroundNormalColor;

    public void OnClick_OpenPromocodePanel()
    {
        UI_GlobalManager.instance.UI_mainMenu.SetMainMenuOpened(false);
        PromocodesPanel_OnOpen();
    }
    public void OnClick_ClosePromocodePanel()
    {
        UI_GlobalManager.instance.UI_mainMenu.SetMainMenuOpened(true);
        PromocodesPanel_OnClose();
    }

    Tween promocodeModal_ScaleInTween;
    Tween promocodeModal_ScaleOutTween;
    Tween promocodeBack_FadeInTween;
    Tween promocodeBack_FadeOutTween;
    void PromocodesPanel_OnOpen()
    {
        ResetAllSettingsTweens();
        promocodeInputField.SetTextWithoutNotify("");
        promocodeBackgroundPanelHolder.gameObject.SetActive(true);
        promocodeBackgroundPanel.gameObject.SetActive(true);
        promocodeBackgroundPanel.color = new Color(promocodeBackgroundPanel.color.r, promocodeBackgroundPanel.color.g, promocodeBackgroundPanel.color.b, 0f);
        promocodeModalPanel.transform.localScale = Vector3.zero;

        promocodeModal_ScaleInTween = promocodeModalPanel.transform.DOScale(Vector3.one, UI_GlobalManager.instance.UI_mainMenu.modalWindowOpenTime).SetEase(UI_GlobalManager.instance.UI_mainMenu.modalWindowScaleInEase);
        promocodeBack_FadeInTween = promocodeBackgroundPanel.DOColor(promocodeBackgroundNormalColor, UI_GlobalManager.instance.UI_mainMenu.modalWindowOpenTime);
    }

    void PromocodesPanel_OnClose()
    {
        ResetAllSettingsTweens();
        promocodeBackgroundPanel.color = promocodeBackgroundNormalColor;
        promocodeModalPanel.transform.localScale = Vector3.one;

        promocodeModal_ScaleOutTween = promocodeModalPanel.transform.DOScale(Vector3.zero, UI_GlobalManager.instance.UI_mainMenu.modalWindowCloseTime).SetEase(UI_GlobalManager.instance.UI_mainMenu.modalWindowScaleOutEase);
        promocodeBack_FadeOutTween = promocodeBackgroundPanel.DOColor(new Color(promocodeBackgroundNormalColor.r, promocodeBackgroundNormalColor.g, promocodeBackgroundNormalColor.b, 0f), UI_GlobalManager.instance.UI_mainMenu.modalWindowCloseTime).OnComplete(() =>
        {
            promocodeBackgroundPanelHolder.gameObject.SetActive(false);
            promocodeBackgroundPanel.gameObject.SetActive(false);
        });
    }

    void ResetAllSettingsTweens()
    {
        DOTween.Kill(promocodeModal_ScaleInTween);
        DOTween.Kill(promocodeModal_ScaleOutTween);
        DOTween.Kill(promocodeBack_FadeInTween);
        DOTween.Kill(promocodeBack_FadeOutTween);
    }

    [Space(5f)]
    public TMP_InputField promocodeInputField;
    string promocodeText = "";

    public void OnEdit_Promocode(string text) { promocodeText = text; }

    public void OnClick_SubmitPromocode()
    {
        if(IsStringClearFromErrors(promocodeText, null, Input_Field.Promocode, 1, 25))
        {
            ConnectionManager.instance.SendMessageToServer($"{PROMOCODE_FROM_CLIENT}|{promocodeText}");
        }
        promocodeInputField.SetTextWithoutNotify("");
        promocodeText = "";
    }

    #endregion
}
