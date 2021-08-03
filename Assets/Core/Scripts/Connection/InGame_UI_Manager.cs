using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGame_UI_Manager : MonoBehaviour
{
    public void OnClick_LeavePlayRoom()
    {
        OnlineGameManager.instance.OnPlayRoomExited();
        ConnectionManager.instance.OnClick_LeavePlayroom();
    }
}
