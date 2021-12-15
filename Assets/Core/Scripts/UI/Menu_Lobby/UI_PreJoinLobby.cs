using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static NetworkingMessageAttributes;
using static Util_UI;
using static DataTypes;
using static EnumsAndData;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

public class UI_PreJoinLobby : MonoBehaviour
{
    Playroom assignedPlayroom;
    string passwordEntered;

    public Image backgroundImage;
    GameObject selectedGameObj;

    [Header("Tween settings")]
    public float openCloseTime = 0.3f;
    public Ease openCloseEase = Ease.Linear;
    Color normalBackgroundColor;

    public void SetUp(Playroom playroom)
    {
        normalBackgroundColor = backgroundImage.color;
        assignedPlayroom = playroom;
        if (assignedPlayroom.isPublic)
        {
            passwordedPanel.SetActive(false);
            freeToJoinPanel.SetActive(true);
            selectedGameObj = freeToJoinPanel;
        }
        else
        {
            passwordedPanel.SetActive(true);
            freeToJoinPanel.SetActive(false);
            selectedGameObj = passwordedPanel;
        }

        backgroundImage.color = new Color(normalBackgroundColor.r, normalBackgroundColor.g, normalBackgroundColor.b, 0f);
        backgroundImage.DOColor(normalBackgroundColor, openCloseTime);

        selectedGameObj.transform.localScale = Vector3.zero;
        selectedGameObj.transform.DOScale(Vector3.one, openCloseTime);
    }

    public void OnEditText_EnterPassword(string pass)
    {
        passwordEntered = pass;
    }

    public GameObject passwordedPanel;
    public GameObject freeToJoinPanel;
    // "enter_playroom|3251|the_greatest_password_ever";
    public void OnClick_JoinLobby()
    {
        if (assignedPlayroom.isPublic)
        {
            ConnectionManager.instance.SendMessageToServer($"{ENTER_PLAY_ROOM}|{assignedPlayroom.id}");
        }
        else
        {
            if (!IsStringClearFromErrors(passwordEntered, null, Input_Field.Password, 5, 15))
                return;

            ConnectionManager.instance.SendMessageToServer($"{ENTER_PLAY_ROOM}|{assignedPlayroom.id}|{passwordEntered}");
        }

        if (!oneTimeClose) return;
        oneTimeClose = false;
        Sequence sq = DOTween.Sequence();
        sq.Append(backgroundImage.DOFade(0f, openCloseTime)).Append(selectedGameObj.transform.DOScale(Vector3.zero, openCloseTime)).OnComplete(() => {
            Destroy(gameObject);
        });
    }

    bool oneTimeClose = true;
    public void OnClick_Cancel()
    {
        if (!oneTimeClose) return;
        oneTimeClose = false;

        backgroundImage.color = normalBackgroundColor;
        selectedGameObj.transform.localScale = Vector3.one;

        Sequence sq = DOTween.Sequence();
        sq.Append(backgroundImage.DOFade(0f, openCloseTime)).Append(selectedGameObj.transform.DOScale(Vector3.zero, openCloseTime)).OnComplete(() => {
            Destroy(gameObject);
        });
    }
}
