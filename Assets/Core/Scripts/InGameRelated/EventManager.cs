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
    public static bool sendCoordinatesToServer;
    public static Vector3 spawnPositionFromServer;

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
    public void MakeActualShot(Vector3 projectileSpawnPoint, Quaternion rotation, GameObject gameObjectToIgnore, int dbIdOfPlayerWhoMadeShot)
    {
        // for muzzle flash
        Player player = gameObjectToIgnore.GetComponent<Player>();
        GameObject particles = Instantiate(PrefabsHolder.instance.electricMuzzleFlash_prefab, player.projectileSpawnPoint.position, player.projectileSpawnPoint.rotation);
        particles.transform.SetParent(player.projectileSpawnPoint);

        GameObject projectile = Instantiate(PrefabsHolder.instance.gravityProjectile_prefab, projectileSpawnPoint, rotation);
        GravityProjectile gravP = projectile.GetComponent<GravityProjectile>();
        gravP.LaunchProjectile(gameObjectToIgnore, dbIdOfPlayerWhoMadeShot);
    }

    public void OnTriggerEnter(Collider other) // level death zone collider
    {
        PlayerMovementController movementController = other.GetComponent<PlayerMovementController>();
        if (movementController != null && EventManager.isAlive)
        {
            EventManager.isAlive = false;
            MC = movementController;
            StartCoroutine(KillPlayer(DeathDetails.FellOutOfMap, 0.2f));
            camSimpleFollow.SetFalling(true);
        }
    }
    // "player_died|killer_ip|reasonOfDeath
    public IEnumerator KillPlayer(DeathDetails deathDetails, float initialDelay = 0)
    {
        int killerDbId = MC.dbIdOflastHitPlayer;
        //if (killerDbId.Equals("")) killerDbId = "none";

        if (MC.hitAssignedToPlayer != null) StopCoroutine(MC.hitAssignedToPlayer);
        MC.dbIdOflastHitPlayer = -1;

        yield return new WaitForSeconds(initialDelay);
        EventManager.isAlive = false;
        MC.KillPlayer();

        ConnectionManager.instance.SendMessageToServer($"{PLAYER_DIED}|{killerDbId}|{deathDetails}");
        yield return new WaitForSeconds(2f);

        sendCoordinatesToServer = false;
    }
}
