using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NetworkingMessageAttributes;

public class SpikeTrap : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    PlayerMovementController mc;
    public static int randomSpawnPosIndex;

    public List<Material> randomSpikeColors;
    public bool useRandomColors = true;

    private void Start()
    {
        if (useRandomColors)
        {
            meshRenderer.material = randomSpikeColors[UnityEngine.Random.Range(0, randomSpikeColors.Count)];
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerMovementController movementController = collision.gameObject.GetComponent<PlayerMovementController>();
        if (movementController != null)
        {
            mc = movementController;
            randomSpawnPosIndex = UnityEngine.Random.Range(0, EventManager.instance.spawnPositions.Count);
            Invoke(nameof(KillPlayer), 0.2f);

            Debug.Log("Flag 1");
        }
        else
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log("Opponent collided with spike trap");
                StartCoroutine(SetDeathStatus(player));
            }
            else
            {
                Debug.Log("Error 1");
            }
        }
    }
    // "player_died|killer_ip|reasonOfDeath
    void KillPlayer()
    {
        mc.KillPlayer();

        ConnectionManager.instance.SendMessageToServer($"{PLAYER_DIED}|null|null");
    }

    IEnumerator SetDeathStatus(Player player)
    {
        yield return new WaitForSeconds(2.5f);
        player.playerData.deathStatus = 1;
    }
}
