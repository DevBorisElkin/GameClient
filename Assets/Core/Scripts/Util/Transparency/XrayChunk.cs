using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XrayChunk : MonoBehaviour
{
    [HideInInspector] public List<MeshRenderer> xRayChunk;

    private void Awake()
    {
        xRayChunk = new List<MeshRenderer>();
        for (int i = 0; i < transform.childCount; i++)
            xRayChunk.Add(transform.GetChild(i).GetComponent<MeshRenderer>());
        _SetBlocksVisible(true);
    }

    public void SetBlocksVisible(bool state)
    {
        _SetBlocksVisible(state);
        if (!state)
        {
            StopAllCoroutines();
            StartCoroutine(ResetToVisibleState());
        }
    }

    void _SetBlocksVisible(bool state)
    {
        Material chosenMat = state ? PrefabsHolder.instance.mapBasicMat : PrefabsHolder.instance.mapTransparentMat;
        foreach (var a in xRayChunk) a.material = chosenMat;
    }

    IEnumerator ResetToVisibleState()
    {
        yield return new WaitForSeconds(0.5f);
        _SetBlocksVisible(true);
    }
}
