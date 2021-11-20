using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using static OpponentPointer;
using static EnumsAndData;

public class OpponentPointerHandler : MonoBehaviour
{
    public OpponentPointer orderPointer;

    float normalOffset = 80f;
    float normalHintOffset = 155f;  // 155
    
    #region arrow pointer
    public Pointer CreatePointer(Vector3 position)
    {
        OpponentPointer.Pointer pointer = orderPointer.CreatePointer(position);
        FunctionUpdater.Create(() => {
            return false;
        });
        return pointer;
    }

    public void DestroyPointer(Pointer pointerToDestroy)
    {
        if(pointerToDestroy != null)
            orderPointer.DestroyPointer(pointerToDestroy);
    }
    #endregion
    
    public float CalculateCorrectOffset()
    {
        // float minHeight = 630;  // just for information
        float maxHeight = 2000;
        return normalOffset * (Screen.height / maxHeight);  // we get divided value and then multiply normal offset by it to get correct to the screen offset;
    }
}
