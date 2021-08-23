using BorisUnityDev.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static NetworkingMessageAttributes;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    public TMP_Text txt_jumpsLeft;

    public static bool isAlive;
    public static bool sendCoordinatesToServer;
    public static Vector3 spawnPositionFromServer;

    private void Start()
    {
        if (instance != null) Destroy(instance);
        instance = this;

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
    public void MakeActualShot(Vector3 projectileSpawnPoint, Quaternion rotation, GameObject gameObjectToIgnore, string ipOfPlayerWhoWadeShot)
    {
        // for muzzle flash
        Player player = gameObjectToIgnore.GetComponent<Player>();
        GameObject particles = Instantiate(PrefabsHolder.instance.electricMuzzleFlash_prefab, player.projectileSpawnPoint.position, player.projectileSpawnPoint.rotation);
        particles.transform.SetParent(player.projectileSpawnPoint);

        GameObject projectile = Instantiate(PrefabsHolder.instance.gravityProjectile_prefab, projectileSpawnPoint, rotation);
        GravityProjectile gravP = projectile.GetComponent<GravityProjectile>();
        gravP.LaunchProjectile(gameObjectToIgnore, ipOfPlayerWhoWadeShot);
    }

    public void OnTriggerEnter(Collider other)
    {
        PlayerMovementController movementController = other.GetComponent<PlayerMovementController>();
        if (movementController != null)
        {
            MC = movementController;
            Invoke(nameof(KillPlayer), 0.2f);
        }
    }
    // "player_died|killer_ip|reasonOfDeath
    public void KillPlayer()
    {
        EventManager.isAlive = false;
        Invoke(nameof(InvocableDeath), 2f);
        MC.KillPlayer();
    }
    void InvocableDeath()
    {
        sendCoordinatesToServer = false;
        ConnectionManager.instance.SendMessageToServer($"{PLAYER_DIED}|null|null");
    }
}
