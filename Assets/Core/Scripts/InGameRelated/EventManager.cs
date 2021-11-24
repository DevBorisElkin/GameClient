using BorisUnityDev.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static NetworkingMessageAttributes;
using static EnumsAndData;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    public TMP_Text txt_jumpsLeft;

    public static bool isAlive;
    public static bool isAvailableForRaycaster;
    public static bool sendCoordinatesToServer;
    public static bool isWaitingFor5secStart;
    public static Vector3 spawnPositionFromServer;

    public float showMovementAfterDeath = 2.3f;

    [HideInInspector] public CamSimpleFollow camSimpleFollow;

    private void Start()
    {
        if (instance != null) Destroy(instance);
        instance = this;

        camSimpleFollow = FindObjectOfType<CamSimpleFollow>();
        OnlineGameManager.instance.OnPlayRoomEntered();
        OnlineGameManager.instance.SpawnPlayer(spawnPositionFromServer);
    }

    private void OnDestroy()
    {
        OnlineGameManager.instance.OnPlayRoomExited();
    }
    public void OnClick_TryToJump()
    {
        OnlineGameManager.instance.playerMovementConetroller.TryToJump_Request();
    }

    PlayerMovementController mc;
    public PlayerMovementController MC
    {
        get
        {
            if(mc == null) mc = FindObjectOfType<PlayerMovementController>();
            return mc;
        }
        set { if (mc == null) mc = value; }
    }
    float x_offset_for_additional_shot = 2.5f;
    public void MakeActualShot(Vector3 projectileSpawnPoint, Quaternion rotation, GameObject gameObjectToIgnore, int dbIdOfPlayerWhoMadeShot, List<Rune> activeRuneModifiers)
    {
        // for muzzle flash
        Player player = gameObjectToIgnore.GetComponent<Player>();
        GameObject particles = Instantiate(PrefabsHolder.instance.electricMuzzleFlash_prefab, player.projectileSpawnPoint.position, player.projectileSpawnPoint.rotation);
        particles.transform.SetParent(player.projectileSpawnPoint);
        var muzzleFlash = particles.GetComponent<GravityProjectileMuzzleFlash>();
        muzzleFlash.SetUp(activeRuneModifiers);

        // check for triple shot
        if (!activeRuneModifiers.Contains(Rune.RedViolet))
            MakeSingleShot(projectileSpawnPoint, rotation, gameObjectToIgnore, dbIdOfPlayerWhoMadeShot, activeRuneModifiers);
        else
        {
            // TODO move left and right first and third shots

            Vector3 forward = Quaternion.Euler(0, rotation.eulerAngles.y, 0) * Vector3.forward;
            Vector3 left = Quaternion.Euler(0, -90, 0) * forward;
            Vector3 right = Quaternion.Euler(0, 90, 0) * forward;

            MakeSingleShot(projectileSpawnPoint + (left * x_offset_for_additional_shot), rotation, gameObjectToIgnore, dbIdOfPlayerWhoMadeShot, activeRuneModifiers);
            MakeSingleShot(projectileSpawnPoint, rotation, gameObjectToIgnore, dbIdOfPlayerWhoMadeShot, activeRuneModifiers);
            MakeSingleShot(projectileSpawnPoint + (right * x_offset_for_additional_shot), rotation, gameObjectToIgnore, dbIdOfPlayerWhoMadeShot, activeRuneModifiers);
        }
    }

    void MakeSingleShot(Vector3 spawnPos, Quaternion rotation, GameObject objToIgnore, int dbIdOfShooter, List<Rune> activeRuneModifiers)
    {
        GameObject projectile = Instantiate(PrefabsHolder.instance.gravityProjectile_prefab, spawnPos, rotation);
        GravityProjectile gravP = projectile.GetComponent<GravityProjectile>();
        gravP.LaunchProjectile(objToIgnore, dbIdOfShooter, activeRuneModifiers);
    }

    public void OnTriggerEnter(Collider other) // level death zone collider
    {
        PlayerMovementController movementController = other.GetComponent<PlayerMovementController>();
        if (movementController != null && EventManager.isAlive)
        {
            EventManager.isAlive = false;
            MC = movementController;
            StartCoroutine(KillPlayer(DeathDetails.FellOutOfMap, 0f));
            camSimpleFollow.SetFalling(true);
        }
    }
    // "player_died|killer_ip|reasonOfDeath
    public IEnumerator KillPlayer(DeathDetails deathDetails, float initialDelay = 0)
    {
        VibrationsManager.OnLocalPlayerDies_Vibrations();
        // tmp
        if (deathDetails == DeathDetails.BlackRuneKilled) deathDetails = DeathDetails.FellOutOfMap;

        CameraRenderingManager.instance.SetRedRuneDebuffState(false);

        EventManager.isAlive = false;
        int killerDbId = MC.dbIdOflastHitPlayer;

        if (MC.hitAssignedToPlayer != null) StopCoroutine(MC.hitAssignedToPlayer);
        MC.dbIdOflastHitPlayer = -1;

        yield return new WaitForSeconds(initialDelay);
        EventManager.isAlive = false;
        MC.KillPlayer();

        ConnectionManager.instance.SendMessageToServer($"{PLAYER_DIED}|{killerDbId}|{deathDetails}");
        yield return new WaitForSeconds(showMovementAfterDeath);

        sendCoordinatesToServer = false;
    }

    public IEnumerator SetIsAvailableForRaycaster()
    {
        yield return new WaitForSeconds(1f);
        isAvailableForRaycaster = true;
    }
}
