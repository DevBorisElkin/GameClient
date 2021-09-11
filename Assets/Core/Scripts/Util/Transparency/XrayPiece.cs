using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XrayPiece : MonoBehaviour
{
    [HideInInspector] public XrayChunk assignedChunk;

    private void Awake()
    {
        assignedChunk = transform.parent.GetComponent<XrayChunk>();
    }
}
