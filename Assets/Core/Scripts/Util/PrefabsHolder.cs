using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumsAndData;
using static DataTypes;

public class PrefabsHolder : MonoBehaviour
{
    public static PrefabsHolder instance;
    private void Awake()
    {
        InitSingleton();
    }

    void InitSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(gameObject);
    }

    public Material mapBasicMat;
    public Material mapTransparentMat;

    public GameObject player_prefab;
    public GameObject opponent_prefab;
    public GameObject localPlayerGhost_prefab;
    public GameObject playerDeathParticles_prefab;

    public GameObject electricMuzzleFlash_prefab;
    public GameObject electrizedObject_prefab;
    public GameObject gravityProjectile_prefab;
    public GameObject gravityProjectile_explosion;

    public GameObject ui_preJoinLobby_prefab;
    public GameObject ui_lobbyItem_prefab;
    public GameObject ui_createLobby_prefab;
    public GameObject ui_messageFromServer_prefab;

    public GameObject ui_playerInLobbyItem_prefab;

    public GameObject ui_inGameEventMessage;

    public GameObject ui_opponentPointer_prefab;

    public GameObject rune_prefab;

    public List<RuneIcon> runeIcons;

    public Sprite GetSpriteByRuneType(Rune rune)
    {
        foreach(var a in runeIcons)
            if (a.rune == rune) return a.sprite;
        return null;
    }
}
