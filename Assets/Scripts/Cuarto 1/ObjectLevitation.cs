using UnityEngine;

public class ObjectLevitation : MonoBehaviour
{
    [Header("Levitation Settings")]
    [SerializeField] private float amplitude = 0.2f;   // Altura del movimiento

    [SerializeField] private float speed = 2f;          // Velocidad de levitación

    private Vector3 startPosition;

    private void Start()
    {
        // Guardamos la posición inicial
        startPosition = transform.position;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(
            startPosition.x,
            startPosition.y + yOffset,
            startPosition.z
        );
    }
}