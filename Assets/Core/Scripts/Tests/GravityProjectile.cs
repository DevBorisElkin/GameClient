using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityProjectile : MonoBehaviour
{
    public float speed = 3f;
    bool active;
    Rigidbody rb;

    public GameObject playerToIgnore;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void LaunchProjectile(GameObject _playerToIgnore)
    {
        playerToIgnore = _playerToIgnore;
        active = true;
    }
    private void FixedUpdate()
    {
        if (!active) return;
        rb.MovePosition(transform.position + (transform.forward * Time.deltaTime * speed));
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject != playerToIgnore)
        {
            //Debug.Log($"Projectile hit: {collision.collider.name}");
            Instantiate(PrefabsHolder.instance.gravityProjectile_explosion, collision.GetContact(0).point - transform.forward * 0.1f, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
