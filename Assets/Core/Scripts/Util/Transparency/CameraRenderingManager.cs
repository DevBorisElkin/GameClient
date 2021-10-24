using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OnlineGameManager;
using DG.Tweening;
using UnityEngine.UI;

public class CameraRenderingManager : MonoBehaviour
{
    public static CameraRenderingManager instance;

    Camera _camera;
    int xRayObjLayer;
    int coremapLayer;
    float dontWorkForDeadOpponents = 4.5f;

    public OpponentPointerHandler opponentPointerHandler;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;

        _camera = Camera.main;
        xRayObjLayer = LayerMask.GetMask("XrayObject", "Player");
        coremapLayer = LayerMask.GetMask("XrayObject", "Player", "CoreMapLayer");

        SetInstantRedRuneDebuffImageState(false);
    }

    private void Update()
    {
        if (!EventManager.isAvailableForRaycaster) return;
        foreach(PlayerData a in OnlineGameManager.instance.opponents)
        {
            if(a.controlledGameObject != null)
            {
                ManageRaycast(a);
                ManageOpponentPointer(a);
            }
        }

        //Debug.DrawRay(_camera.transform.position, _camera.ScreenPointToRay(_camera.WorldToScreenPoint(OnlineGameManager.instance.player.transform.position)).direction * 30f);
        if (EventManager.isAvailableForRaycaster && Physics.Raycast(_camera.ScreenPointToRay(_camera.WorldToScreenPoint(OnlineGameManager.instance.playerGameObj.transform.position)), out RaycastHit hit2, 100, xRayObjLayer))
        {
            //Debug.Log("f"+hit2.collider.gameObject.name);
            if (hit2.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
            {
                XrayPiece piece = hit2.collider.GetComponent<XrayPiece>();
                if (piece != null)
                {
                    piece.assignedChunk.SetBlocksVisible(false);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        ManageRedRuneEffectDebuff();
    }

    void ManageRaycast(PlayerData a)
    {
        if ((DateTime.Now - a.lastTimeDead).TotalSeconds <= dontWorkForDeadOpponents) return;

        if (Physics.Raycast(_camera.ScreenPointToRay(_camera.WorldToScreenPoint(a.controlledGameObject.transform.position)), out RaycastHit hit1, 100, xRayObjLayer))
        {
            //Debug.Log("f"+hit2.collider.gameObject.name);
            if (hit1.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
            {
                XrayPiece piece = hit1.collider.GetComponent<XrayPiece>();
                if (piece != null)
                {
                    if (Physics.Raycast(a.controlledGameObject.transform.position, _camera.transform.position - a.controlledGameObject.transform.position, out RaycastHit hit3, 100, coremapLayer))
                    {
                        if (hit3.collider.gameObject.layer != LayerMask.NameToLayer("CoreMapLayer"))
                            piece.assignedChunk.SetBlocksVisible(false);
                    }
                }
            }
        }
    }

    void ManageOpponentPointer(PlayerData a)
    {
        if (a.opponentPointer != null)
        {
            a.opponentPointer.targetPosition = a.position;
            a.opponentPointer.Update();
        }
        else
        {
            a.opponentPointer = opponentPointerHandler.CreatePointer(a.position);
        }
    }

    #region RedRune debuff

    [Header("RedRuneEffect")]
    public RectTransform redRuneRectTransform;
    public Image redRuneDebuffImage;

    public Color defaultColor;
    public Color turnedOffColor;

    public float redRuneEffectLerpSpeed = 15f;
    public float fullRotDur = 30;

    bool redRuneDebuffActive;
    float redRuneTurnOffOnTime = 0.4f;

    int idOfFadeInOutTween = -1;
    int idOfRotatingTween = -2;

    
    public void SetInstantRedRuneDebuffImageState(bool state)
    {
        if (state)
            redRuneDebuffImage.color = defaultColor;
        else redRuneDebuffImage.color = turnedOffColor;

        redRuneDebuffActive = state;
    }

    public void SetRedRuneDebuffState(bool state)
    {
        if (state == redRuneDebuffActive) return;

        DOTween.Kill(idOfFadeInOutTween);
        DOTween.Kill(idOfRotatingTween);

        if (state)
        {
            SetProperEffectScreenPosition();
            DOTween.To(() => redRuneDebuffImage.color, x => redRuneDebuffImage.color = x, defaultColor, redRuneTurnOffOnTime).id = idOfFadeInOutTween;
            redRuneRectTransform.DORotate(new Vector3(0, 0, 360), fullRotDur, RotateMode.LocalAxisAdd).SetLoops(-1);
        }
        else
        {
            DOTween.To(() => redRuneDebuffImage.color, x => redRuneDebuffImage.color = x, turnedOffColor, redRuneTurnOffOnTime).id = idOfFadeInOutTween;
        }

        redRuneDebuffActive = state;
    }

    void SetProperEffectScreenPosition()
    {
        if (OnlineGameManager.instance != null && OnlineGameManager.instance.playerGameObj != null)
            redRuneRectTransform.position = _camera.WorldToScreenPoint(OnlineGameManager.instance.playerGameObj.transform.position);
    }

    void ManageRedRuneEffectDebuff()
    {
        if (!redRuneDebuffActive) return;

        if (OnlineGameManager.instance != null && OnlineGameManager.instance.playerGameObj != null)
            redRuneRectTransform.position = Vector3.Lerp(redRuneRectTransform.position, 
                _camera.WorldToScreenPoint(OnlineGameManager.instance.playerGameObj.transform.position), redRuneEffectLerpSpeed);
    }

    #endregion
}
