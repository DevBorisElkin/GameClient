using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamSimpleFollow : MonoBehaviour
{
    public Vector3 offset;
    public float lerpSpeed = 3f;

    public Transform transformToFollow;

    Quaternion baseRotation;

    [Header("Falling out confition")]
    public bool useRandomCamAnimOnFall = true;
    bool isFalling;
    public Vector3 fallingOffset;
    public float fallingPositionLerpSpeed = 3f;
    public float fallingRotationLerpSpeed = 1f;
    [Space(5f)]
    public float camBehingPlayerOnFall = 5f;
    public float camAbovePlayerOnFall = 17f;

    public CamFollowOnDeath camFollowOnDeath;
    [HideInInspector] public NicknameCanvas nicknamePlayerCanvas;

    private void Start()
    {
        baseRotation = transform.rotation;
        if(FindObjectOfType<PlayerMovementController>() != null)
            if (transformToFollow == null) transformToFollow = FindObjectOfType<PlayerMovementController>().transform;
    }

    void FixedUpdate()
    {
        if (transformToFollow != null)
        {
            if(!isFalling)
                transform.position = Vector3.Lerp(transform.position, transformToFollow.position + offset, lerpSpeed * Time.deltaTime);
            else if (isFalling)
            {
                if (camFollowOnDeath.Equals(CamFollowOnDeath.SimpleFall))
                {
                    transform.position = Vector3.Lerp(transform.position, transformToFollow.position + fallingOffset, fallingPositionLerpSpeed * Time.deltaTime);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((transformToFollow.position - transform.position).normalized), fallingRotationLerpSpeed);
                }
                else if (camFollowOnDeath.Equals(CamFollowOnDeath.RotateWithPlayer))
                {
                    transform.position = Vector3.Lerp(transform.position, transformToFollow.position + (-transformToFollow.forward * camBehingPlayerOnFall) + Vector3.up * camAbovePlayerOnFall, fallingPositionLerpSpeed * Time.deltaTime);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((transformToFollow.position - transform.position).normalized), fallingRotationLerpSpeed);
                }
            }
        }
    }
    public void SetFalling(bool newState)
    {
        if (newState && useRandomCamAnimOnFall) camFollowOnDeath = (CamFollowOnDeath)UnityEngine.Random.Range(0, 2);
        if (!newState) transform.rotation = baseRotation;
        if (nicknamePlayerCanvas != null) nicknamePlayerCanvas.isMainPlayerFalling = newState;
        isFalling = newState;
    }


    public enum CamFollowOnDeath { SimpleFall = 0, RotateWithPlayer = 1}

    /* way 2
     * 
     public Vector3 offset;
    public float lerpSpeed = 3f;

    public Transform transformToFollow;

    Quaternion baseRotation;

    [Header("Falling out confition")]
    bool isFalling;
    public Vector3 fallingOffset;
    public float fallingPositionLerpSpeed = 3f;
    public float fallingRotationLerpSpeed = 1f;
    

    private void Start()
    {
        baseRotation = transform.rotation;
        if(FindObjectOfType<PlayerMovementController>() != null)
            if (transformToFollow == null) transformToFollow = FindObjectOfType<PlayerMovementController>().transform;
    }

    void FixedUpdate()
    {
        if (transformToFollow != null)
        {
            if(!isFalling)
                transform.position = Vector3.Lerp(transform.position, transformToFollow.position + offset, lerpSpeed * Time.deltaTime);
            else if (isFalling)
            {
                Quaternion lookRot = Quaternion.LookRotation((transformToFollow.position - transform.position).normalized);
                transform.position = Vector3.Lerp(transform.position, transformToFollow.position + (-transformToFollow.forward * camBehingPlayerOnFall) + Vector3.up * camAbovePlayerOnFall, fallingPositionLerpSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, fallingRotationLerpSpeed);
                    
            }
        }
    }
    public void SetFalling(bool newState)
    {
        isFalling = newState;
        if (!newState) transform.rotation = baseRotation;
    }
     */
}
