using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static OnlineGameManager;

public class NicknameCanvas : MonoBehaviour
{
    private GameObject lookAtTarget = null;

    private Vector3 TargetPosition;

    public Vector3 offset;

    Player player;
    PlayerData playerData;

    public bool activated;
    public bool isMainPlayerFalling;

    private void Update()
    {
        if (!activated) return;

        if(player == null)
        {
            Destroy(gameObject);
            return;
        }
        
        transform.position = player.transform.position + offset;
    }

    private void LateUpdate()
    {
        Quaternion targetRotation = Quaternion.LookRotation(lookAtTarget.transform.position - transform.position);
        if(!isMainPlayerFalling) targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, 180, targetRotation.eulerAngles.z);
        transform.rotation = targetRotation;
    }

    public void SetUp(Player _player, PlayerData _playerData)
    {
        if (!_playerData.copyOfLocalPlayer)
        {
            transform.SetParent(null);

            if (lookAtTarget == null)
                lookAtTarget = Camera.main.gameObject;

            player = _player;
            playerData = _playerData;
            nicknameTxt.text = playerData.nickname;

            activated = true;

            // we know it's attached to the core player
            if (_playerData.db_id == ConnectionManager.instance.currentUserData.db_id) EventManager.instance.camSimpleFollow.nicknamePlayerCanvas = this;
        }
        else Destroy(gameObject);
    }
    public TMP_Text nicknameTxt;
}
