using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeArea : MonoBehaviour
{
    RectTransform panel;
    Rect safeAreaRect;

    public bool useSafeRepeat = true;

    void Awake()
    {
        panel = GetComponent<RectTransform>();

        if (panel == null)
        {
            Debug.LogError("Cannot apply safe area - no RectTransform found on " + name);
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(SafeAreaAction());
    }

    private void OnEnable()
    {
        StartCoroutine(SafeAreaAction());
    }

    IEnumerator SafeAreaAction()
    {
        yield return new WaitForEndOfFrame();

        safeAreaRect = Screen.safeArea;
        ApplySafeArea(safeAreaRect);

        if (useSafeRepeat)
        {
            yield return new WaitForSeconds(0.5f);
            safeAreaRect = Screen.safeArea;
            ApplySafeArea(safeAreaRect);
        }
    }

    void ApplySafeArea(Rect r)
    {
        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        if(panel.anchorMin != anchorMin || panel.anchorMax != anchorMax)
        {
            Debug.Log($"ApplySafeArea [{name}]");
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
        }

    //     Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}", name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
    }
}