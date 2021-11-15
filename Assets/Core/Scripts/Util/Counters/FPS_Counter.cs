using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPS_Counter : MonoBehaviour
{
    [SerializeField] public TMP_Text txt;

    public float refresht = 0.5f;
    int frameCounter = 0;
    float timeCounter = 0.0f;
    float lastFramerate = 0.0f;

    void Update()
    {
        if (timeCounter < refresht)
        {
            timeCounter += Time.deltaTime;
            frameCounter++;
        }
        else
        {
            lastFramerate = (float)frameCounter / timeCounter;
            int lastfrInt = (int)lastFramerate;

            txt.text = "FPS: " + lastfrInt.ToString();

            frameCounter = 0;
            timeCounter = 0.0f;

        }

    }
}
