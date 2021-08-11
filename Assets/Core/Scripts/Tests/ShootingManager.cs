using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingManager : MonoBehaviour
{
    public void MakeActualShot(Vector3 projectileSpawnPoint, Quaternion rotation, GameObject gameObjectToIgnore)
    {
        GameObject projectile = Instantiate(PrefabsHolder.instance.gravityProjectile_prefab, projectileSpawnPoint, rotation);
        GravityProjectile gravP = projectile.GetComponent<GravityProjectile>();
        gravP.LaunchProjectile(gameObjectToIgnore);
    }
}
