using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DataTypes;
using static EnumsAndData;

public class RuneInstance : MonoBehaviour
{
    public int runeId;
    public Rune runeType;

    [Space(5f)]
    public MeshRenderer meshRenderer;

    public List<RuneVisual> runeVisuals;
    [SerializeField] public List<RuneColor> runeColors;

    public void SetUpRune(int runeId, Rune runeType)
    {
        this.runeId = runeId;
        this.runeType = runeType;

        ManageVisuals();
    }

    void ManageVisuals()
    {
        foreach(var a in runeVisuals)
        {
            if (a.runeType != runeType) a.gameObject.SetActive(false);
            else a.gameObject.SetActive(true);
        }
        Material mat = GetProperMaterial();
        if (mat != null) meshRenderer.material = mat;
    }

    Material GetProperMaterial()
    {
        foreach(var a in runeColors)
            if (a.rune == runeType) return a.material;
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        var pmc = other.gameObject.GetComponent<PlayerMovementController>();
        if (pmc == null) return;

        // here we send request, trying to pick up the rune
        OnlineGameManager.instance.SendMessage_PlayerTriesToPickUpRune(runeId, runeType);
    }
}
