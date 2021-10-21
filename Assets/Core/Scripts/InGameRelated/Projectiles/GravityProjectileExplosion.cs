using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumsAndData;


public class GravityProjectileExplosion : MonoBehaviour
{
    public List<RuneVisual> runeVisuals;
    [HideInInspector] public List<Rune> activeRuneEffects;

    private void Start() { }
    public void SetUp(List<Rune> activeRuneEffects)
    {
        this.activeRuneEffects = activeRuneEffects;
        AdjustVisuals();
    }

    void AdjustVisuals()
    {
        if (activeRuneEffects.Count == 0)
        {
            foreach (var a in runeVisuals)
                if (a.runeType == Rune.None) a.gameObject.SetActive(true);
        }
        else
        {
            foreach (var a in runeVisuals)
            {
                if (activeRuneEffects.Contains(Rune.Black))
                { if (a.runeType == Rune.Black) a.gameObject.SetActive(true); }
                else if (activeRuneEffects.Contains(Rune.Red))
                { if (a.runeType == Rune.Red) a.gameObject.SetActive(true); }
                else { if (a.runeType == Rune.None) a.gameObject.SetActive(true); }
            }
        }
    }
}
