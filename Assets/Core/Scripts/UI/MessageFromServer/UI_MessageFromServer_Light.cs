using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EnumsAndData;
using DG.Tweening;
using UniRx;

public class UI_MessageFromServer_Light : MonoBehaviour
{
    [Space(5f)]
    public Color simpleColor;
    public Color warningColor;
    public Color errorColor;

    [Space(5f)]
    public GameObject messageBody;
    public Image backgroundImage;
    public TMP_Text messageTxt;
    public int splitMessageBy = 40;

    [Header("Tween Settings")]
    public float fadeDelay = 1.5f;
    public float moveUpTime = 4f;
    public float totalMoveUpValue = 15f;
    public Color transparentColor;
    public Ease globalEase = Ease.Linear;

    MessageFromServer_MessageType messageType;
    string message;

    Tween moveTween;
    Tween fadeTextTween;
    Tween fadeImageTween;

    public void SetUp(string message, MessageFromServer_MessageType messageType)
    {
        this.messageType = messageType;
        this.message = SplitStringByNewLine(message, splitMessageBy);
        messageTxt.text = this.message;

        SetUpCorrectStyles();
    }

    private void Awake()
    {
        Observable.Timer(TimeSpan.FromSeconds(fadeDelay)).Subscribe(_ => {
            backgroundImage.DOFade(0f, moveUpTime - fadeDelay).SetEase(globalEase);
            messageTxt.DOFade(0f, moveUpTime - fadeDelay).SetEase(globalEase);
        });
        messageBody.transform.DOMove(messageBody.transform.position + new Vector3(0, totalMoveUpValue, 0), moveUpTime).SetEase(globalEase).OnComplete(() => {
            ResetTweens();
            Destroy(gameObject);
        });
    }

    void ResetTweens()
    {
        if (moveTween != null)
        {
            moveTween.Pause();
            moveTween = null;
        }
        if (fadeTextTween != null)
        {
            fadeTextTween.Pause();
            fadeTextTween = null;
        }
        if (fadeImageTween != null)
        {
            fadeImageTween.Pause();
            fadeImageTween = null;
        }
    }

    void SetUpCorrectStyles()
    {
        if (messageType == MessageFromServer_MessageType.Info)
        {
            backgroundImage.color = simpleColor;

        }
        else if (messageType == MessageFromServer_MessageType.Warning)
        {
            backgroundImage.color = warningColor;

        }
        else if (messageType == MessageFromServer_MessageType.Error)
        {
            backgroundImage.color = errorColor;

        }
    }

    public static string SplitStringByNewLine(string source, int interval = 40)
    {
        try
        {
            List<string> incomingList = new List<string>();
            List<string> resultList = SplitString(incomingList, 0, source, interval);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < resultList.Count; i++)
            {
                if (i != 0) sb.Append("\n");
                sb.Append(resultList[i]);
            }
            return sb.ToString();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return source;
        }
    }

    public static List<string> SplitString(List<string> incomingList, int startingIndex, string source, int minInterval)
    {
        int nextInvolvedCharsCount = startingIndex + minInterval;
        if (nextInvolvedCharsCount >= source.Length)
        {
            incomingList.Add(source.Substring(startingIndex, source.Length - startingIndex));
            return incomingList;
        }

        string part_1 = source.Substring(startingIndex, minInterval);
        string part_2 = source.Substring(startingIndex + minInterval, source.Length - (startingIndex + minInterval));

        if (part_2.Contains(" "))
        {
            int indexOfSpace = part_2.IndexOf(' ');
            incomingList.Add(source.Substring(startingIndex, minInterval + indexOfSpace));
            startingIndex = startingIndex + minInterval + indexOfSpace + 1;
            return SplitString(incomingList, startingIndex, source, minInterval);
        }
        else
        {
            incomingList.Add(source.Substring(startingIndex, source.Length - startingIndex));
            return incomingList;
        }
    }
}
