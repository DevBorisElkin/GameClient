using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OnlineGameManager;

public class CamXrayRaycaster : MonoBehaviour
{
    Camera _camera;
    int xRayObjLayer;
    private void Awake()
    {
        _camera = Camera.main;
        xRayObjLayer = LayerMask.GetMask("XrayObject", "Player");
    }

    private void Update()
    {
        foreach(PlayerData a in OnlineGameManager.instance.opponents)
        {
            if(a.controlledGameObject != null)
            {
                if (Physics.Raycast(_camera.ScreenPointToRay(_camera.WorldToScreenPoint(a.controlledGameObject.transform.position)), out RaycastHit hit1, 100, xRayObjLayer))
                {
                    //Debug.Log("f"+hit2.collider.gameObject.name);
                    if (hit1.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                    {
                        XrayPiece piece = hit1.collider.GetComponent<XrayPiece>();
                        if (piece != null)
                        {
                            piece.assignedChunk.SetBlocksVisible(false);
                        }
                    }
                }
            }
        }
        //Debug.DrawRay(_camera.transform.position, _camera.ScreenPointToRay(_camera.WorldToScreenPoint(OnlineGameManager.instance.player.transform.position)).direction * 30f);
        if (Physics.Raycast(_camera.ScreenPointToRay(_camera.WorldToScreenPoint(OnlineGameManager.instance.player.transform.position)), out RaycastHit hit2, 100, xRayObjLayer))
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
}
