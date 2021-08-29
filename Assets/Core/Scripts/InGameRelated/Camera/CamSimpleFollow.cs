using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamSimpleFollow : MonoBehaviour
{
    public Vector3 offset;
    public float lerpSpeed = 3f;

    public Transform transformToFollow;

    private void Start()
    {
        if(FindObjectOfType<PlayerMovementController>() != null)
            if (transformToFollow == null) transformToFollow = FindObjectOfType<PlayerMovementController>().transform;
    }

    void Update()
    {
        if (transformToFollow != null)
            transform.position = Vector3.Lerp(transform.position, transformToFollow.position + offset, lerpSpeed * Time.deltaTime);
    }
}
