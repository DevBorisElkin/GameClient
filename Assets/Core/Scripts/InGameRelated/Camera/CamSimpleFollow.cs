using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamSimpleFollow : MonoBehaviour
{
    public CinemachineBrain cin_brain;
    public CinemachineVirtualCamera cin_cam_main;
    public CinemachineVirtualCamera cin_cam_falling_simple;
    public CinemachineVirtualCamera cin_cam_falling_playerRot;
    public CinemachineVirtualCamera cin_cam_aboveTheMap;

    [Space(5f)]
    public CinemachineBlendDefinition.Style defaultBlendStyle = CinemachineBlendDefinition.Style.EaseIn;
    public CinemachineBlendDefinition.Style defaultBlendFallStyle = CinemachineBlendDefinition.Style.EaseIn;
    public float defaultBlendTime = 1f;
    public float delayBeforeZoomInFromAbove = 0.5f;
    [Space(5f)]
    CinemachineTransposer cin_transposer;
    CinemachineComposer cin_composer;

    List<CinemachineVirtualCamera> allCameras;

    public Vector3 offset;
    public Vector3 fallingOffset_Simple;
    public Vector3 fallingOffset_PlayerDirection;

    public float lerpSpeed = 3f;

    Transform transformToFollow;

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
        allCameras = new List<CinemachineVirtualCamera>();
        allCameras.Add(cin_cam_main);
        allCameras.Add(cin_cam_falling_simple);
        allCameras.Add(cin_cam_falling_playerRot);
        allCameras.Add(cin_cam_aboveTheMap);

        transformToFollow = FindObjectOfType<PlayerMovementController>().transform;

        if (transformToFollow != null)
        {
            cin_transposer = cin_cam_main.GetCinemachineComponent<CinemachineTransposer>();
            cin_composer = cin_cam_main.GetCinemachineComponent<CinemachineComposer>();

            cin_cam_falling_simple.Follow = transformToFollow;
            cin_cam_falling_simple.LookAt = transformToFollow;
            cin_cam_falling_playerRot.Follow = transformToFollow;
            cin_cam_falling_playerRot.LookAt = transformToFollow;

            BasicSetUp();
        }
    }

    public void SetFalling(bool newState)
    {
        if (newState && useRandomCamAnimOnFall) camFollowOnDeath = (CamFollowOnDeath)UnityEngine.Random.Range(0, 2);
        if (nicknamePlayerCanvas != null) nicknamePlayerCanvas.isMainPlayerFalling = newState;
        isFalling = newState;

        if (isFalling)
        {
            if (camFollowOnDeath.Equals(CamFollowOnDeath.SimpleFall))
            {
                Falling_Simple_SetUp();
            }
            else if (camFollowOnDeath.Equals(CamFollowOnDeath.RotateWithPlayer))
            {
                Falling_PlayerDir_SetUp();
            }
        }
        else if (!isFalling)
        {
            StartCoroutine(InvocableDelayedSetUp());
        }
    }

    IEnumerator InvocableDelayedSetUp()
    {
        InstantTransmissionSetUp();
        SetPrioritiveCamera(cin_cam_aboveTheMap);
        yield return new WaitForSeconds(delayBeforeZoomInFromAbove);
        BasicSetUp();
    }

    void InstantTransmissionSetUp(bool fast = false)
    {
        if (fast)
        {
            cin_brain.m_DefaultBlend.m_Style = defaultBlendFallStyle;
            cin_brain.m_DefaultBlend.m_Time = 1f;
        }
        else
        {
            cin_brain.m_DefaultBlend.m_Style = defaultBlendFallStyle;
            cin_brain.m_DefaultBlend.m_Time = 0f;
        }
        
        cin_transposer.m_XDamping = 0;
        cin_transposer.m_YDamping = 0;
        cin_transposer.m_ZDamping = 0;
        cin_composer.m_HorizontalDamping = 0f;
        cin_composer.m_VerticalDamping = 0f;
    }

    void BasicSetUp()
    {
        cin_brain.m_DefaultBlend.m_Style = defaultBlendStyle;
        cin_brain.m_DefaultBlend.m_Time = defaultBlendTime;
        SetPrioritiveCamera(cin_cam_main);
        cin_transposer.m_XDamping = 1.3f;
        cin_transposer.m_YDamping = 1.3f;
        cin_transposer.m_ZDamping = 1.3f;
        cin_transposer.m_FollowOffset = offset;
        cin_composer.m_HorizontalDamping = 0.5f;
        cin_composer.m_VerticalDamping = 0.5f;
        cin_composer.m_DeadZoneWidth = 0.0f;
        cin_composer.m_DeadZoneHeight = 0.0f;
        cin_composer.m_TrackedObjectOffset = new Vector3(0, -4.00f, 0);

        cin_cam_main.Follow = transformToFollow;
        cin_cam_main.LookAt = transformToFollow;
        cin_transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;

        StartCoroutine(CameraHardBordersFix());
    }

    void Falling_Simple_SetUp()
    {
        InstantTransmissionSetUp(true);
        SetPrioritiveCamera(cin_cam_falling_simple);
    }

    void Falling_PlayerDir_SetUp()
    {
        InstantTransmissionSetUp(true);
        SetPrioritiveCamera(cin_cam_falling_playerRot);
    }

    void SetPrioritiveCamera(CinemachineVirtualCamera newCoreCamera)
    {
        foreach(var a in allCameras)
            if (a != newCoreCamera) a.Priority = 10;
        newCoreCamera.Priority = 12;
    }


    public enum CamFollowOnDeath { SimpleFall = 0, RotateWithPlayer = 1}


    public IEnumerator CameraHardBordersFix()
    {
        yield return new WaitForSeconds(defaultBlendTime + 0.1f);
        cin_composer.m_DeadZoneWidth = 0.2f;
        cin_composer.m_DeadZoneHeight = 0.2f;
    }

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
