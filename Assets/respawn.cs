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
        // Se imprime/ejecuta 2 veces pq tamb toma el trigger del groundCheck
        Debug.Log("TP");
        playerTf.position = respawnTeleport.position;
    }
}