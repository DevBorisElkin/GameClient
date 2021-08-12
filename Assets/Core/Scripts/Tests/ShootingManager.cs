using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingManager : MonoBehaviour
{
    public void MakeActualShot(Vector3 projectileSpawnPoint, Quaternion rotation, GameObject gameObjectToIgnore)
    {
        // for muzzle flash
        Player player = gameObjectToIgnore.GetComponent<Player>();
        GameObject particles = Instantiate(PrefabsHolder.instance.electricMuzzleFlash_prefab, player.projectileSpawnPoint.position, player.projectileSpawnPoint.rotation);
        particles.transform.SetParent(player.projectileSpawnPoint);

        GameObject projectile = Instantiate(PrefabsHolder.instance.gravityProjectile_prefab, projectileSpawnPoint, rotation);
        GravityProjectile gravP = projectile.GetComponent<GravityProjectile>();
        gravP.LaunchProjectile(gameObjectToIgnore);
    }
}
