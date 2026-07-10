using UnityEngine;

public class respawn : MonoBehaviour
{
    [SerializeField]
    private Transform respawnTeleport;

#warning Hardcoded...

    [SerializeField]
    private Transform playerTf;

    private void OnTriggerEnter(Collider other)
    {
        playerTf.position = respawnTeleport.position;
        respawnTeleport.GetComponent<ParticleSystem>().Play();
    }
}