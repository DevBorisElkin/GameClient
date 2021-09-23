using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamSimpleFollow : MonoBehaviour
{
    CinemachineVirtualCamera cin_cam;
    CinemachineTransposer cin_transposer;
    CinemachineComposer cin_composer;

    public Vector3 offset;
    public Vector3 fallingOffset;

    public float lerpSpeed = 3f;

    public Transform transformToFollow;

    Quaternion baseRotation;

    [Header("Falling out confition")]
    public bool useRandomCamAnimOnFall = true;
    bool isFalling;
    
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

        cin_cam = FindObjectOfType<CinemachineVirtualCamera>();

        if(transformToFollow != null && cin_cam != null)
        {
            cin_transposer = cin_cam.GetCinemachineComponent<CinemachineTransposer>();
            cin_composer = cin_cam.GetCinemachineComponent<CinemachineComposer>();

            BasicSetUp();
        }
    }

    //void FixedUpdate()
    //{
    //    if (transformToFollow != null)
    //    {
    //        if(!isFalling)
    //            transform.position = Vector3.Lerp(transform.position, transformToFollow.position + offset, lerpSpeed * Time.deltaTime);
    //        else if (isFalling)
    //        {
    //            if (camFollowOnDeath.Equals(CamFollowOnDeath.SimpleFall))
    //            {
    //                transform.position = Vector3.Lerp(transform.position, transformToFollow.position + fallingOffset, fallingPositionLerpSpeed * Time.deltaTime);
    //                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((transformToFollow.position - transform.position).normalized), fallingRotationLerpSpeed);
    //            }
    //            else if (camFollowOnDeath.Equals(CamFollowOnDeath.RotateWithPlayer))
    //            {
    //                transform.position = Vector3.Lerp(transform.position, transformToFollow.position + (-transformToFollow.forward * camBehingPlayerOnFall) + Vector3.up * camAbovePlayerOnFall, fallingPositionLerpSpeed * Time.deltaTime);
    //                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((transformToFollow.position - transform.position).normalized), fallingRotationLerpSpeed);
    //            }
    //        }
    //    }
    //}
    public void SetFalling(bool newState)
    {
        if (newState && useRandomCamAnimOnFall) camFollowOnDeath = (CamFollowOnDeath)UnityEngine.Random.Range(0, 2);
        if (!newState) transform.rotation = baseRotation;
        if (nicknamePlayerCanvas != null) nicknamePlayerCanvas.isMainPlayerFalling = newState;
        isFalling = newState;

        if (isFalling)
        {
            
        }
        else if (!isFalling)
        {
            
            StartCoroutine(InvocableDelayedSetUp());
        }

    }

    IEnumerator InvocableDelayedSetUp()
    {
        InstantTransmissionSetUp();
        yield return new WaitForSeconds(0.5f);
        BasicSetUp();
    }

    void InstantTransmissionSetUp()
    {
        cin_transposer.m_XDamping = 0;
        cin_transposer.m_YDamping = 0;
        cin_transposer.m_ZDamping = 0;
        cin_composer.m_HorizontalDamping = 0f;
        cin_composer.m_VerticalDamping = 0f;
    }

    void BasicSetUp()
    {
        cin_transposer.m_XDamping = 1.3f;
        cin_transposer.m_YDamping = 1.3f;
        cin_transposer.m_ZDamping = 1.3f;
        cin_transposer.m_FollowOffset = offset;
        cin_composer.m_HorizontalDamping = 0.5f;
        cin_composer.m_VerticalDamping = 0.5f;
        cin_composer.m_DeadZoneWidth = 0.2f;
        cin_composer.m_DeadZoneHeight = 0.2f;
        cin_cam.Follow = transformToFollow;
        cin_cam.LookAt = transformToFollow;
        cin_transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
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
