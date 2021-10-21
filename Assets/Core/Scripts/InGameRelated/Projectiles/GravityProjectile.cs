using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumsAndData;
using static DataTypes;

public class GravityProjectile : MonoBehaviour
{
    public float speed = 3f;
    bool active;
    Rigidbody rb;

    public GameObject playerToIgnore;
    public int dbIdOfPleyerWhoMadeLaunch;

    [Space(5f)]
    public List<RuneVisual> runeVisuals;

    [HideInInspector] public List<Rune> activeRuneEffects;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void LaunchProjectile(GameObject _playerToIgnore, int _dbIdOfPlayerWhoMadeLaunch, List<Rune> activeRuneEffects)
    {
        playerToIgnore = _playerToIgnore;
        dbIdOfPleyerWhoMadeLaunch = _dbIdOfPlayerWhoMadeLaunch;
        this.activeRuneEffects = activeRuneEffects;
        AdjustVisuals();
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
            if(collision.collider.gameObject.GetComponent<Player>()!= null)
            {
                GameObject spawnedObject = Instantiate(PrefabsHolder.instance.electrizedObject_prefab, collision.gameObject.transform.position, collision.gameObject.transform.rotation);
                spawnedObject.transform.SetParent(collision.transform);
            }
            //Debug.Log($"Projectile hit: {collision.collider.name}");
            Instantiate(PrefabsHolder.instance.gravityProjectile_explosion, collision.GetContact(0).point - transform.forward * 0.1f, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    void AdjustVisuals()
    {
        foreach (var a in activeRuneEffects) Debug.Log(a);

        if(activeRuneEffects.Count == 0)
        {
            foreach(var a in runeVisuals)
                if (a.runeType != Rune.None) a.gameObject.SetActive(false);
        }
        else
        {
            foreach(var a in runeVisuals)
            {
                if (activeRuneEffects.Contains(Rune.Black))
                { if (a.runeType != Rune.Black) a.gameObject.SetActive(false); }
                else if (activeRuneEffects.Contains(Rune.Red))
                { if (a.runeType != Rune.Red) a.gameObject.SetActive(false); }
                else { if (a.runeType != Rune.None) a.gameObject.SetActive(false); }
            }
        }
    }
}
