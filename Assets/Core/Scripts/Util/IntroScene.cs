using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScene : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadSceneAsync("MainScene");
    }
}
