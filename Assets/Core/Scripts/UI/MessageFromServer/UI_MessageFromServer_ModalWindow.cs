using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EnumsAndData;
using DG.Tweening;

public class UI_MessageFromServer_ModalWindow : MonoBehaviour
{
    public Image backgroundImage;
    public Image modalWindowImage;
    public TMP_Text headerTxt;
    public TMP_Text messageTxt;
    public TMP_Text closeBtnTxt;
    public Image closeBtnImage;

    [Space(5f)]
    public Sprite messageModalWindowSprite;
    public Sprite warningMessageModalWindowSprite;
    public Sprite errorMessageModalWindowSprite;
    [Space(3f)]
    public Sprite messageButtonSprite;
    public Sprite warningMessageButtonSprite;
    public Sprite errorMessageButtonSprite;
    [Space(3f)]
    public Color messageButtonColor;
    public Color warningButtonColor;
    public Color errorButtonColor;

    [Space(3f)]
    public Ease backgroundFadeInEase = Ease.Linear;
    public Ease backgroundFadeOutEase = Ease.Linear;
    public Ease modalWindowScaleInEase = Ease.Linear;
    public Ease modalWindowScaleOutEase = Ease.Linear;

    MessageFromServer_MessageType messageType;
    string message;

    bool oneTimeOpen = true;
    bool oneTimeClose = true;

    public void SetUp(string message, MessageFromServer_MessageType messageType)
    {
        this.messageType = messageType;
        this.message = message;
        messageTxt.text = message;
        SetUpCorrectStyles();
    }

    private void Awake() => Animation_OnOpen();
    public void OnClick_Close() => Animation_OnClose();

    void SetUpCorrectStyles()
    {
        if(messageType == MessageFromServer_MessageType.Info)
        {
            modalWindowImage.sprite = messageModalWindowSprite;
            closeBtnImage.sprite = messageButtonSprite;
            headerTxt.text = "Message:";
            closeBtnTxt.text = "Okay";
            closeBtnImage.color = messageButtonColor;
        }
        else if(messageType == MessageFromServer_MessageType.Warning)
        {
            modalWindowImage.sprite = warningMessageModalWindowSprite;
            closeBtnImage.sprite = warningMessageButtonSprite;
            headerTxt.text = "Warning:";
            closeBtnTxt.text = "Close";
            closeBtnImage.color = warningButtonColor;
        }
        else if (messageType == MessageFromServer_MessageType.Error)
        {
            modalWindowImage.sprite = errorMessageModalWindowSprite;
            closeBtnImage.sprite = errorMessageButtonSprite;
            headerTxt.text = "Error:";
            closeBtnTxt.text = "Close";
            closeBtnImage.color = errorButtonColor;
        }
    }
    [Header("AnimationRelated")]
    public Color backgroundFadedInColor;
    public Color backgroundFadedOutColor;
    public float modalWindowAppearTime = 0.8f;
    public float modalWindowDisappearTime = 0.8f;
    public float backgroundFadeInTime = 0.7f;
    public float backgroundFadeOutTime = 0.7f;

    Tween backgroundColorInTween;
    Tween modalWindowScaleInTween;

    Tween backgroundColorOutTween;
    Tween modalWindowScaleOutTween;
    void Animation_OnOpen()
    {
        if (!oneTimeOpen) return;
        oneTimeOpen = false;

        ResetAllTweens();

        backgroundImage.color = backgroundFadedOutColor;
        backgroundColorInTween = backgroundImage.DOColor(backgroundFadedInColor, backgroundFadeInTime).SetEase(backgroundFadeInEase);

        modalWindowImage.transform.localScale = Vector3.zero;
        modalWindowScaleInTween = modalWindowImage.transform.DOScale(Vector3.one, modalWindowAppearTime).SetEase(modalWindowScaleInEase); ;
    }

    void Animation_OnClose()
    {
        if (!oneTimeClose) return;
        oneTimeClose = false;

        ResetAllTweens();

        backgroundImage.color = backgroundFadedInColor;
        backgroundColorOutTween = backgroundImage.DOColor(backgroundFadedOutColor, backgroundFadeOutTime).SetEase(backgroundFadeOutEase);

        modalWindowImage.transform.localScale = Vector3.one;
        modalWindowScaleOutTween = modalWindowImage.transform.DOScale(Vector3.zero, modalWindowDisappearTime).SetEase(modalWindowScaleOutEase).OnComplete(() => {
            Destroy(gameObject);
        });
    }

    void ResetAllTweens()
    {
        if (backgroundColorInTween != null && backgroundColorInTween.IsPlaying())
        {
            backgroundColorInTween.Pause();
            backgroundColorInTween = null;
        }
        if (modalWindowScaleInTween != null && modalWindowScaleInTween.IsPlaying())
        {
            modalWindowScaleInTween.Pause();
            modalWindowScaleInTween = null;
        }
        if (backgroundColorOutTween != null && backgroundColorOutTween.IsPlaying())
        {
            backgroundColorOutTween.Pause();
            backgroundColorOutTween = null;
        }
        if (modalWindowScaleOutTween != null && modalWindowScaleOutTween.IsPlaying())
        {
            modalWindowScaleOutTween.Pause();
            modalWindowScaleOutTween = null;
        }
    }
}
