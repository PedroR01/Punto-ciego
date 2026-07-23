using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
internal struct Bubble
{
    public GameObject bubble;
    public Volume groupVolume;
    public Light[] lights;
}

public class BubbleFeedback : MonoBehaviour
{
    [Header("Orden de burbujas")]
    [SerializeField]
    private Bubble[] bubbles;

    private int currentBubbleIndex = 0;

    private void OnEnable()
    {
        CuartoDosGM.OnCorrectState += OnBubbleInteracted;
    }

    private void OnDisable()
    {
        CuartoDosGM.OnCorrectState -= OnBubbleInteracted;
    }

    public void OnBubbleInteracted(InteractionContext actionTarget)
    {
        // Verificar si es el locker correcto
        if (bubbles[currentBubbleIndex].bubble.transform != actionTarget.target)
        {
            Debug.Log("Burbuja incorrecta.");
            return;
        }

        Debug.Log($"Burbuja correcta: {actionTarget.target}");
        BubbleTrigger(bubbles[currentBubbleIndex].bubble, bubbles[currentBubbleIndex].groupVolume);

        // Encender las luces del siguiente
        if (currentBubbleIndex < bubbles.Length - 1)
        {
            currentBubbleIndex++;
            foreach (var light in bubbles[currentBubbleIndex].lights)
            {
                light.enabled = true;
                bubbles[currentBubbleIndex].bubble.SetActive(true);
                bubbles[currentBubbleIndex].groupVolume.enabled = true;
                Debug.Log($"Encendiendo la luz - {light} - de {bubbles[currentBubbleIndex].bubble}");
            }
        }
        else
            Debug.Log("Todas las burbujas activadas.");
    }

    private void BubbleTrigger(GameObject bubbleOff, Volume volumeOff)
    {
        bubbleOff.GetComponent<BoxCollider>().enabled = false;
        bubbleOff.GetComponentInChildren<MeshRenderer>().enabled = false;
        volumeOff.enabled = false;
    }
}