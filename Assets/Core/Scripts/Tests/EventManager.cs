using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [SerializeField]
    public List<SpawnPosition> spawnPositions = new List<SpawnPosition>();

    public TMP_Text txt_jumpsLeft;
    private void Start()
    {
        OnlineGameManager.instance.OnPlayRoomEntered();
        OnlineGameManager.instance.SpawnPlayer(spawnPositions);
    }

    private void OnDestroy()
    {
        OnlineGameManager.instance.OnPlayRoomExited();
    }

    [System.Serializable]
    public class SpawnPosition
    {
        public int index;
        public GameObject spawnPos;
    }
    public void OnClick_TryToJump()
    {
        OnlineGameManager.instance.playerMovementConetroller.TryToJump_Request();
    }

    PlayerMovementController mc;
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
            mc = movementController;
            randowSpawnPosIndex = UnityEngine.Random.Range(0, spawnPositions.Count);
            Invoke(nameof(KillPlayer), 0.2f);
        }
        else
        {
            Player player = other.GetComponent<Player>();
            if(player != null)
            {
                Debug.Log("Opponent entered death zone");
                StartCoroutine(SetDeathStatus(player));
            }
        }
    }
    public static int randowSpawnPosIndex;
    void KillPlayer()
    {
        mc.KillPlayer();

        // TODO tell the server that player has been killed and the latest hitter
    }

    IEnumerator SetDeathStatus(Player player)
    {
        yield return new WaitForSeconds(2.5f);
        player.playerData.deathStatus = 1;
    }
}
