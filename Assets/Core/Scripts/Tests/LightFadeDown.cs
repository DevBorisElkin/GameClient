using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFadeDown : MonoBehaviour
{
    Light light;

    public float waitUntilStartFading = 0.3f;
    public float totalFadeTime = 1f;
    public float iterationTime = 0.1f;

    void Start()
    {
        light = GetComponent<Light>();
        StartCoroutine(LightFadeCoroutine());
    }

    IEnumerator LightFadeCoroutine()
    {
        yield return new WaitForSeconds(waitUntilStartFading);

        float removeEachIteration = light.intensity / (totalFadeTime / iterationTime);
        while(light.intensity > 0)
        {
            if (light.intensity - removeEachIteration > 0)
                light.intensity -= removeEachIteration;
            else
            {
                light.intensity = 0;
                yield break;
            }

            yield return new WaitForSeconds(iterationTime);
        }
    }
}
