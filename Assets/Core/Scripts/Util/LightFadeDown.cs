using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFadeDown : MonoBehaviour
{
    Light lightComponent;

    public float waitUntilStartFading = 0.3f;
    public float totalFadeTime = 1f;
    public float iterationTime = 0.1f;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        StartCoroutine(LightFadeCoroutine());
    }

    IEnumerator LightFadeCoroutine()
    {
        yield return new WaitForSeconds(waitUntilStartFading);

        float removeEachIteration = lightComponent.intensity / (totalFadeTime / iterationTime);
        while(lightComponent.intensity > 0)
        {
            if (lightComponent.intensity - removeEachIteration > 0)
                lightComponent.intensity -= removeEachIteration;
            else
            {
                lightComponent.intensity = 0;
                yield break;
            }

            yield return new WaitForSeconds(iterationTime);
        }
    }
}
