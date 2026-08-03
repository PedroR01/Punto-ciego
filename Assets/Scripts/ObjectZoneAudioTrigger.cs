using System.Collections;
using UnityEngine;

public class ObjectZoneAudioTrigger : MonoBehaviour
{
    [Tooltip("Tag que debe tener el jugador para activar el audio")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Tiempo de espera antes de repetir el audio")]
    [SerializeField] private float delayBetweenPlays = 2f;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private bool isInteractable = true;

    private bool playerInside = false;
    private Coroutine playRoutine;

    private void Start()
    {
        if(isInteractable)
        this.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;

        // Si no hay una rutina corriendo, la iniciamos
        if (this.enabled && playRoutine == null)
        {
            playRoutine = StartCoroutine(PlayLoopRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;
    }

    private IEnumerator PlayLoopRoutine()
    {
        while (playerInside)
        {
            // Reproduce el clip si no está sonando
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            // Espera hasta que termine de reproducirse
            yield return new WaitUntil(() => !audioSource.isPlaying);

            // Si el jugador sigue dentro, espera el delay antes de repetir
            if (playerInside)
            {
                yield return new WaitForSeconds(delayBetweenPlays);
            }
        }

        playRoutine = null;
    }
}