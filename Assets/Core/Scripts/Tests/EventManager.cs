using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
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
            Invoke(nameof(KillPlayer), 0.2f);
        }
    }

    void KillPlayer()
    {
        mc.KillPlayer();


        // TODO tell the server that player has been killed and the latest hitter


    }
}
