using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject gravityProjectilePrefab;
    public Transform projectileSpawnPoint;

    public float reloadTime = 1.4f;
    bool canShoot;

    private void Start()
    {
        canShoot = true;
    }

    public void TryToShoot()
    {
        if (!canShoot) return;
        canShoot = false;

        GameObject projectile = Instantiate(gravityProjectilePrefab, projectileSpawnPoint.position, transform.rotation);
        GravityProjectile gravP = projectile.GetComponent<GravityProjectile>();
        gravP.LaunchProjectile(gameObject);
        StartCoroutine(ReloadCoroutine());
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject projectile = Instantiate(gravityProjectilePrefab, projectileSpawnPoint.position, transform.rotation);
            GravityProjectile gravP = projectile.GetComponent<GravityProjectile>();
            gravP.LaunchProjectile(gameObject);
        }
    }

    IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(reloadTime);
        canShoot = true;
    }
}
