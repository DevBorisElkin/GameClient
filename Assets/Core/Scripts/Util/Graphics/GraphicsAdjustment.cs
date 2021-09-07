using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumsAndData;

public class GraphicsAdjustment : MonoBehaviour
{
    public List<GameObject> applyOnHighGraphics;
    public List<GameObject> applyOnMediumGraphics;
    public List<GameObject> applyOnLowGraphics;


    void Start()
    {
        DisableAll();
        ApplyGraphics();
    }

    void DisableAll()
    {
        if(applyOnHighGraphics != null)
            foreach (var a in applyOnHighGraphics)
                a.SetActive(false);
        if (applyOnMediumGraphics != null)
            foreach (var a in applyOnMediumGraphics)
                a.SetActive(false);
        if (applyOnLowGraphics != null)
            foreach (var a in applyOnLowGraphics)
                a.SetActive(false);
    }

    public void ApplyGraphics()
    {
        int currentGraphicsValue = PlayerPrefs.GetInt(CODE_GRAPHICS_SETTINGS, 0);
        if (currentGraphicsValue == 0)
        {
            if (applyOnHighGraphics != null)
                foreach (var a in applyOnHighGraphics)
                    a.SetActive(true);
        }
        else if (currentGraphicsValue == 1)
        {
            if (applyOnMediumGraphics != null)
                foreach (var a in applyOnMediumGraphics)
                    a.SetActive(true);
        }
        else if (currentGraphicsValue == 2)
        {
            if (applyOnLowGraphics != null)
                foreach (var a in applyOnLowGraphics)
                    a.SetActive(true);
        }
    }
}
