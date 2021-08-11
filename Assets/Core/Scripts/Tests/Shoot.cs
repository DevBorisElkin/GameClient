using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject gravityProjectilePrefab;
    public Transform projectileSpawnPoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject projectile = Instantiate(gravityProjectilePrefab, projectileSpawnPoint.position, transform.rotation);
            GravityProjectile gravP = projectile.GetComponent<GravityProjectile>();
            gravP.LaunchProjectile(gameObject);
        }
    }
}
