using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static EnumsAndData;

public class UI_Settings : MonoBehaviour
{
    // 0 = high, 1 = medium, 2 = low

    public TMP_Dropdown dropdownGraphics;

    int currentGraphicsValue;

    public void OnPanelOpened()
    {
        currentGraphicsValue = PlayerPrefs.GetInt(CODE_GRAPHICS_SETTINGS, 0);
        dropdownGraphics.SetValueWithoutNotify(currentGraphicsValue);
    }
    public void OnGraphicsChanged(int newGraphicsValue)
    {
        if(currentGraphicsValue != newGraphicsValue)
        {
            PlayerPrefs.SetInt(CODE_GRAPHICS_SETTINGS, newGraphicsValue);
            QualitySettings.SetQualityLevel(ConvertChosenGraphicsToCorrectInt(newGraphicsValue));
        }
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
}
