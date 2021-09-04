using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NetworkingMessageAttributes;
using static EnumsAndData;

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
        if (movementController != null && EventManager.isAlive)
        {
            EventManager.isAlive = false;
            if (mc == null) mc = movementController;
            StartCoroutine(EventManager.instance.KillPlayer(DeathDetails.TouchedSpikes, 0));
        }
    }
}
