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
                transform.position = Vector3.Lerp(transform.position, transformToFollow.position + fallingOffset, fallingPositionLerpSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation((transformToFollow.position - transform.position).normalized), fallingRotationLerpSpeed);
            }
        }
    }
    public void SetFalling(bool newState)
    {
        isFalling = newState;
        if (!newState) transform.rotation = baseRotation;
    }
}
