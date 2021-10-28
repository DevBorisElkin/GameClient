using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform transformToFollow;
    public Vector3 offset;

    private void Start() { }

    private void FixedUpdate()
    {
        transform.position = transformToFollow.position + offset;
    }
}
