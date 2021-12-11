using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static EnumsAndData;
using static Util_UI;
using static NetworkingMessageAttributes;
using UnityEngine.UI;

public class UI_Settings : MonoBehaviour
{
    #region Main
    // 0 = high, 1 = medium, 2 = low

    public TMP_Dropdown dropdownGraphics;
    public Toggle toggleVibrations;
    public SwitchToggle switchToggleVibrations;

    int currentGraphicsValue;
    bool boolVibrationsValue;

    public void OnPanelOpened()
    {
        currentGraphicsValue = PlayerPrefs.GetInt(CODE_GRAPHICS_SETTINGS, 0);
        dropdownGraphics.SetValueWithoutNotify(currentGraphicsValue);

        boolVibrationsValue = PlayerPrefs.GetInt(CODE_VIBRATIONS_SETTINGS, 1) > 0 ? true : false;
        toggleVibrations.SetIsOnWithoutNotify(boolVibrationsValue);
        switchToggleVibrations.OnSwitchInstant(boolVibrationsValue);
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
        boolVibrationsValue = newValue;
        PlayerPrefs.SetInt(CODE_VIBRATIONS_SETTINGS, boolVibrationsValue ? 1 : 0);
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
