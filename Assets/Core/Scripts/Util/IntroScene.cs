using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EnumsAndData;

public class IntroScene : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.SetQualityLevel(UI_Settings.ConvertChosenGraphicsToCorrectInt(PlayerPrefs.GetInt(CODE_GRAPHICS_SETTINGS, 0)));
        VibrationsManager.hapticsEnabled = PlayerPrefs.GetInt(CODE_VIBRATIONS_SETTINGS, 1) > 0 ? true : false;
        SceneManager.LoadSceneAsync("MainScene");
    }

    void ActivateVibrations() => Handheld.Vibrate();
}
